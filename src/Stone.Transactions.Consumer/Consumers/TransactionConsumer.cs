using Confluent.Kafka;
using Elastic.Clients.Elasticsearch.TextStructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Stone.Common.Core.Messages;
using Stone.Common.Extensions;
using Stone.Common.Infrastructure.SearchEngine;
using Stone.Transactions.Consumer.Extensions;
using Stone.Transactions.Domain.Entities;
using System.Text.Json;
using System.Threading.Channels;

namespace Stone.Transactions.Consumer.Consumers
{
    public class TransactionConsumer : BackgroundService
    {
        private const int NumberChannels = 20;       // Channels paralelos. pode ajustar conforme necessário, Via app, estratégia adaptativa, etc

        private readonly ILogger<TransactionConsumer> _logger;
        private readonly AppTransactionsConsumerSettings _settings;
        private readonly IConsumer<string, string> _consumer;
        private readonly ISearchEngine<Transaction> _elasticSearchService;
        private readonly Channel<(string, ConsumeBatch<Transaction>)> _channel;
        private readonly string _groupInstanceId;


        public TransactionConsumer(
            IOptions<AppTransactionsConsumerSettings> settings,
            ILogger<TransactionConsumer> logger,
            ISearchEngine<Transaction> elasticSearchService,
            string containerInstance,
            string consumerName)
        {
            _logger = logger;
            _settings = settings.Value;

             _groupInstanceId = $"transaction-consumer-{consumerName}";

            var config = new ConsumerConfig
            {
                BootstrapServers = _settings.Kafka.BootstrapServers,
                ClientId = $"transaction-consumer-app-{containerInstance}",
                GroupId = "transaction-consumer-group-01",
                GroupInstanceId = _groupInstanceId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnablePartitionEof = true,
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false,
                IsolationLevel = IsolationLevel.ReadCommitted,
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();

            _elasticSearchService = elasticSearchService;

            _channel = Channel.CreateBounded<(string GroupInstanceId, ConsumeBatch<Transaction> Batch)>(
                new BoundedChannelOptions(20)
                {
                    FullMode = BoundedChannelFullMode.Wait // bloqueia se estiver cheio
                }
            );
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            var topic = _settings.Kafka.Topics.TransactionConsumer;

            try
            {
                _consumer.Subscribe(topic);

                _logger.LogInformation($"Consumer {_groupInstanceId} iniciado no tópico {topic}");

                var channelProcessors = Enumerable.Range(0, NumberChannels)
                    .Select(_ => Task.Run(() => ProcessAndPersistBatchesToElasticAsync(stoppingToken)))
                    .ToList();

                var retryKafka = PollyExtensions.RetryKafka(_logger);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {

                        var result = retryKafka.Execute(() => _consumer.Consume(stoppingToken));

                        if (result?.Message?.Value == null)
                            continue;

                        var transactions = JsonSerializer.Deserialize<List<Transaction>>(result.Message.Value);

                        await _channel.Writer.WriteAsync((_groupInstanceId,new ConsumeBatch<Transaction>
                        {
                            ConsumeResult = result,
                            Items = transactions ?? new List<Transaction>()
                        }), stoppingToken);

                    }
                    catch (Exception ex)
                    {
                        // Todas as tentativas falharam, log crítico e notificação

                        _logger.LogError(ex, $"Erro crítico ao consumir mensagens no consumidor {_consumer.Name} após todas as tentativas.");

                        NotifyTeam($"Falha crítica no consumidor {_consumer.Name}: {ex.Message}");

                        await Task.Delay(1000, stoppingToken);
                    }

                }

                // Marca o channel como completo quando parar
                _channel.Writer.Complete();

                // Aguarda todos os processadores finalizarem
                await Task.WhenAll(channelProcessors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao se inscrever no tópico Kafka");
            }
            finally
            {
                _consumer.Close();
                _logger.LogInformation("Consumer finalizado");
            }
        }

        private async Task ProcessAndPersistBatchesToElasticAsync(CancellationToken cancellationToken)
        {
            await foreach (var (groupInstanceId, batch) in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                try
                {

                    var retryElastic = PollyExtensions.RetryElasticAsync(_logger);

                    var resultRetry = await retryElastic.ExecuteAndCaptureAsync(async () =>
                    {
                        await _elasticSearchService.BulkInsertAsync(
                            batch.Items,
                            "transactions-write",
                            bulkSize: 5000,
                            cancellationToken
                        );
                    });

                    if (resultRetry.Outcome != OutcomeType.Failure)
                    {
                        _consumer.Commit(batch.ConsumeResult);
                        _logger.LogInformation($"Consumer {groupInstanceId} envio {batch.Items.Count} para o ES");

                    }
                    else
                    {
                        _logger.LogWarning($"Enviando {batch.Items.Count} mensagens para DLQ...");
                        SendMessageToDeadLetterQueue(batch.Items);

                        // Não podemos confirmar a mensagem, senão perderemos os dados.
                        // nesse caso para fins de exemplo, vamos confirmar para não travar o processamento
                        _consumer.Commit(batch.ConsumeResult);

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Erro ao salvar as transações no elastic: {ex.Message}");

                    throw;
                }
            }
        }

        public void NotifyTeam(string message)
        {
            // Aqui você chama seu método de notificação
            // Pode ser Teams, Slack, SNS, CloudWatch, etc.

            _logger.LogCritical(message);
        }

        public void SendMessageToDeadLetterQueue(List<Transaction> trancations)
        {

            // ou republicar as mensagens para serem consumidas novamente, juntamente com uma estratégia de para evitar loop infinito
        }
    }
}

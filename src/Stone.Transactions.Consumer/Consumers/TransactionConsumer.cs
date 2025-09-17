using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stone.Common.Core.Extensions;
using Stone.Transactions.Consumer.Extensions;

namespace Stone.Transactions.Consumer.Consumers
{
    public class TransactionConsumer : BackgroundService
    {
        private readonly ILogger<TransactionConsumer> _logger;
        private readonly AppTransactionsConsumerSettings _settings;

        private readonly IConsumer<string, string> _consumer;

        public TransactionConsumer(
            ILogger<TransactionConsumer> logger,
            IOptions<AppTransactionsConsumerSettings> settings,
            string containerInstance,
            string consumerName)
        {
            _logger = logger;
            _settings = settings.Value;

            var config = new ConsumerConfig
            {
                BootstrapServers = _settings.Kafka.BootstrapServers,
                ClientId = $"transaction-consumer-app-{containerInstance}",
                GroupId = "transaction-consumer-group-01",
                GroupInstanceId = $"transaction-consumer-{consumerName}",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnablePartitionEof = true,
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false,
                IsolationLevel = IsolationLevel.ReadCommitted,
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var topic = _settings.Kafka.Topics.TransactionConsumer;

            try
            {
                _consumer.Subscribe(topic);

                _logger.LogInformation($"Consumer {_consumer.Name} iniciado no tópico {topic}");

                var retryPolicy = PollyExtensions.RetryKafka(_logger);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {

                        var result = await retryPolicy.ExecuteAsync(() =>
                            Task.Run(() => _consumer.Consume(stoppingToken), stoppingToken)
                         );

                        if (result.IsPartitionEOF)
                            continue;

                        var messsage = "<< Recebida: \t" + result.Message.Value;
                        ProcessMessageAsync();

                        Console.WriteLine(messsage);

                        _consumer.Commit(result);
                        _consumer.StoreOffset(result.TopicPartitionOffset);
                    }
                    catch (Exception ex)
                    {
                        // Todas as tentativas falharam, log crítico e notificação

                        _logger.LogError(ex, $"Erro crítico ao consumir mensagens no consumidor {_consumer.Name} após todas as tentativas.");

                        NotifyTeam($"Falha crítica no consumidor {_consumer.Name}: {ex.Message}");

                        await Task.Delay(1000, stoppingToken);
                    }
                }
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


       public void ProcessMessageAsync()
        {
            Console.WriteLine("Processando mensagem...");
        }

        public void NotifyTeam(string message)
        {
            // Aqui você chama seu método de notificação
            // Pode ser Teams, Slack, SNS, CloudWatch, etc.

            _logger.LogCritical(message);
        }
    }
}

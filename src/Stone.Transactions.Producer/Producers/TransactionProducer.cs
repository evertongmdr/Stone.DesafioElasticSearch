using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stone.Transactions.Domain.Entities;
using Stone.Transactions.Producer.Extensions;
using System.Text;
using System.Text.Json;

namespace Stone.Transactions.Producer.Producers
{
    public interface ITransactionProducer
    {
        Task PublishTransactionsAsync(List<List<Transaction>> batchMessages, CancellationToken cancellationToken = default);
    }

    public class TransactionProducer : ITransactionProducer
    {
        private readonly ILogger<TransactionProducer> _logger;
        private readonly AppTransactionsProducerSettings _settings;
        private readonly IProducer<string, string> _producer;

        public TransactionProducer(ILogger<TransactionProducer> logger, IOptions<AppTransactionsProducerSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;

            var config = new ProducerConfig
            {
                BootstrapServers = _settings.Kafka.BootstrapServers,
                ClientId = "transaction-producer-app-01",
                TransactionalId = $"transaction-producer-01",
                EnableIdempotence = true,
                Acks = Acks.All,
                LingerMs = 10,
                MessageSendMaxRetries = 3,
                CompressionType = CompressionType.Lz4,
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
            _producer.InitTransactions(TimeSpan.FromSeconds(10));
        }

        public async Task PublishTransactionsAsync(List<List<Transaction>> batchMessages, CancellationToken cancellationToken = default)
        {
            try
            {
                _producer.BeginTransaction();

                await Parallel.ForEachAsync(batchMessages, new ParallelOptions
                {
                    MaxDegreeOfParallelism = 6,
                    CancellationToken = cancellationToken
                },

                async (messages, ct) =>
                {

                    var message = new Message<string, string>
                    {
                        Value = JsonSerializer.Serialize(messages),
                        Headers = CreateHeaders()
                    };

                    await _producer.ProduceAsync(_settings.Kafka.Topics.TransactionProducer, message, ct);

                });

                _producer.CommitTransaction();
                _logger.LogInformation("Transação completa enviada com sucesso.");
            }
            catch (Exception ex)
            {
                _producer.AbortTransaction();
                _logger.LogError(ex, "Transação abortada devido a erro: {Message}", ex.Message);

                throw;
            }
        }

        private Headers CreateHeaders()
        {
            return new Headers
            {
                { "application", Encoding.UTF8.GetBytes("TransactionsProducer") },
                { "correlationId", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()) }
            };
        }
    }
}

using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stone.Transactions.Domain.Entities;
using Stone.Transactions.Producer.Extensions;
using System.IO.Compression;
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
        private readonly IProducer<string, byte[]> _producer;

        public TransactionProducer(ILogger<TransactionProducer> logger, IOptions<AppTransactionsProducerSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;

            var config = new ProducerConfig
            {
                BootstrapServers = _settings.Kafka.BootstrapServers,
                EnableIdempotence = true,
                Acks = Acks.All,
                TransactionalId = $"transaction-producer-{Guid.NewGuid()}"
            };

            _producer = new ProducerBuilder<string, byte[]>(config).Build();
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

                    var message = new Message<string, byte[]>
                    {
                        Key = Guid.NewGuid().ToString(),
                        Value = CompressData(messages),
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


        private static byte[] CompressData<T>(T data)
        {
            var serializedBytes = JsonSerializer.SerializeToUtf8Bytes(data);
            using var memoryStream = new MemoryStream();
            using (var zipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
            {
                zipStream.Write(serializedBytes, 0, serializedBytes.Length);
            }
            return memoryStream.ToArray();
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

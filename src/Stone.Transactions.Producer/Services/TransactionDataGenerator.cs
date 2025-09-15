using Microsoft.Extensions.Logging;
using Stone.Transactions.Domain.Entities;
using Stone.Transactions.Producer.Producers;

namespace Stone.Transactions.Producer.Services
{

    public interface ITransactionDataGenerator
    {
        /// <summary>
        /// Gera e envia massa de dados de transações para Kafka.
        /// </summary>
        /// <param name="batchSize">Quantidade de transações por batch.</param>
        /// <param name="delayMs">Delay entre batches em milissegundos.</param>
        public Task GenerateAndPublishDataAsync(int batchSize, int maxBatchesPerSend, int delayMs, CancellationToken cancellationToken = default);
    }

    public class TransactionDataGenerator : ITransactionDataGenerator
    {
        const long TotalMessages = 122713632; // ~20GB de dados
        //const long TotalMessages = 1000; // ~20GB de dados

        private readonly ILogger<TransactionDataGenerator> _logger;
        private readonly ITransactionProducer _producer;

        public TransactionDataGenerator(ILogger<TransactionDataGenerator> logger, ITransactionProducer producer)
        {

            _logger = logger;
            _producer = producer;
        }

        public async Task GenerateAndPublishDataAsync(int batchSize, int maxBatchesPerSend, int delayMs, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Iniciando envio de massa de dados...");

            var random = new Random();
            var clientIds = Enumerable.Range(1, 1000).Select(_ => Guid.NewGuid()).ToList();

            int sentMessages = 0;
            int totalBatches = (int)Math.Ceiling((double)TotalMessages / batchSize);

            for (int i = 0; i < totalBatches; i += maxBatchesPerSend)
            {
                try
                {
                    var batchesToSend = new List<List<Transaction>>();

                    for (int j = 0; j < maxBatchesPerSend && (i + j) < totalBatches; j++)
                    {
                        batchesToSend.Add(GenerateBatch(batchSize, random, clientIds));
                    }

                    sentMessages += batchesToSend.Sum(b => b.Count);
                    DrawProgressBar(sentMessages);

                    await _producer.PublishTransactionsAsync(batchesToSend, cancellationToken);

                    await Task.Delay(delayMs);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"\nErro no envio do batch {i + 1}: {ex.Message}");
                }
            }

            _logger.LogInformation("\nEnvio concluído!");
        }


        public Transaction GenerateRandomTransaction(Random random, List<Guid> clientIds)
        {
            var types = Enum.GetValues(typeof(TransactionType));

            return new Transaction
            {
                Id = Guid.NewGuid(),
                Type = (TransactionType)types.GetValue(random.Next(types.Length)),
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 365)),
                ClientId = clientIds[random.Next(clientIds.Count)],
                PayerId = Guid.NewGuid(),
                Amount = Math.Round((decimal)(random.NextDouble() * 1000), 2)
            };
        }

        private List<Transaction> GenerateBatch(int batchSize, Random random, List<Guid> clientIds)
        {
            var batch = new List<Transaction>(batchSize);

            for (int i = 0; i < batchSize; i++)
            {
                batch.Add(GenerateRandomTransaction(random, clientIds));
            }

            return batch;
        }

        private void DrawProgressBar(long current)
        {
            int width = 50;
            double progress = (double)current / TotalMessages;
            int position = (int)(width * progress);

            Console.Write("[");
            Console.Write(new string('#', position));
            Console.Write(new string('-', width - position));
            Console.Write($"] {progress:P0} ({current}/{TotalMessages} mensagens)\r");
        }
    }

}

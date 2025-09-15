namespace Stone.Transactions.Producer.Extensions
{
    public class AppTransactionsProducerSettings
    {
        public KafkaSettings Kafka { get; set; }
    }

    public class KafkaSettings
    {
        public string BootstrapServers { get; set; }
        public string ClientId { get; set; }
        public KafkaTopics Topics { get; set; }
    }

    public class KafkaTopics
    {
        public string TransactionProducer { get; set; }
    }

}

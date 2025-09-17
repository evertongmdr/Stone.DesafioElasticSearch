namespace Stone.Transactions.Consumer.Extensions
{
    public class AppTransactionsConsumerSettings
    {
        public KafkaSettings Kafka { get; set; }
        public List<ContainerSettings> Containers { get; set; }
    }

    public class KafkaSettings
    {
        public string BootstrapServers { get; set; }
        public KafkaTopics Topics { get; set; }
    }

    public class KafkaTopics
    {
        public string TransactionConsumer { get; set; }
    }

    public class ContainerSettings
    {
        public string Name { get; set; }
        public List<string> ConsumerNames { get; set; }
    }
}

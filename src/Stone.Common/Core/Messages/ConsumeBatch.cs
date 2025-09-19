namespace Stone.Common.Core.Messages
{
    using Confluent.Kafka;

    /// <summary>
    /// Representa um lote de mensagens consumidas do Kafka.
    /// </summary>
    public class ConsumeBatch<T>
    {
        /// <summary>
        /// Resultado original retornado pelo Kafka.
        /// </summary>
        public ConsumeResult<string, string> ConsumeResult { get; set; } = default!;

        /// <summary>
        /// Lote de itens desserializados a partir da mensagem.
        /// </summary>
        public List<T> Items { get; set; } = new();
    }
}

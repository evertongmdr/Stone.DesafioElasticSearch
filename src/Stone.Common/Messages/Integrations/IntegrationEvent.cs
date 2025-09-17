namespace Stone.Common.Messages.Integrations
{
    public abstract class IntegrationEvent
    {
        public string IntegrationEventType { get; protected set; }
        public DateTime Timestamp { get; private set; }

        protected IntegrationEvent()
        {
            IntegrationEventType = GetType().Name;
            Timestamp = DateTime.Now;
        }
    }
}

namespace Stone.Common.Core.Notifications
{
    public abstract class ErrorNotifier
    {
        protected readonly NotificationContext _notificationContext;

        public ErrorNotifier(NotificationContext notificationContext)
        {
            _notificationContext = notificationContext;
        }
    }
}

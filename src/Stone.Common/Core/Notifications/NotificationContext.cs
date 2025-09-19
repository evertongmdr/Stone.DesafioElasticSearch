using FluentValidation.Results;
using Stone.Common.Core.DTOs.Support;
using System.Net;

namespace Stone.Common.Core.Notifications
{
    public class NotificationContext
    {
        private readonly List<Notification> _notifications;
        public IReadOnlyCollection<Notification> Notifications => _notifications;
        public bool ExistNotifications => _notifications.Any();

        public NotificationContext()
        {
            _notifications = new List<Notification>();
        }

        public void AddNotification(string message, HttpStatusCode erroCode = HttpStatusCode.BadRequest)
        {
            _notifications.Add(new Notification(message, erroCode));
        }

        public void AddNotification(Notification notification)
        {
            _notifications.Add(notification);
        }

        public void AddNotifications(ICollection<Notification> notifications)
        {
            _notifications.AddRange(notifications);
        }

        public void AddNotifications(ValidationResult validationResult)
        {
            foreach (var error in validationResult.Errors)
                AddNotification(error.ErrorMessage);

        }

        public void AddNotification(ResponseResult responseResult)
        {
            foreach (var error in responseResult.Errors.Mensagens)
                AddNotification(error, (HttpStatusCode)responseResult.Status);
        }
    }
}

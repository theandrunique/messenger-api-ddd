using MessengerAPI.Domain.UserAggregate.ValueObjects;

namespace MessengerAPI.Application.Common.Interfaces;

public interface INotificationService
{
    /// <summary>
    /// Send notification
    /// </summary>
    /// <param name="jsonMessage">message in json</param>
    /// <param name="recipientIds">list of user ids who will receive notification</param>
    Task NotifyAsync(List<UserId> recipientIds, string jsonMessage);
}

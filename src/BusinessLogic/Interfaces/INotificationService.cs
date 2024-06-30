using Bounan.Common;

namespace Bounan.Bot.BusinessLogic.Interfaces;

public interface INotificationService
{
    Task HandleAsync(VideoDownloadedNotification notification);
}
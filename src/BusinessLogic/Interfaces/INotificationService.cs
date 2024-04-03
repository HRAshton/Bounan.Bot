using Bounan.Bot.BusinessLogic.Models;

namespace Bounan.Bot.BusinessLogic.Interfaces;

public interface INotificationService
{
    Task HandleAsync(BotNotification notification);
}
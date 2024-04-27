namespace Bounan.Bot.BusinessLogic.Interfaces;

public interface IFileIdFinder
{
    Task<string?> GetFileIdAsync(int messageId);
}
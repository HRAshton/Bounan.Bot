using Bounan.Bot.BusinessLogic.Configs;
using Bounan.Bot.BusinessLogic.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace Bounan.Bot.BusinessLogic.Services;

internal class FileIdFinder : IFileIdFinder
{
    private TelegramBotConfig _telegramBotConfig;

    public FileIdFinder(
        ILogger<FileIdFinder> logger,
        ITelegramBotClient botClient,
        IOptions<TelegramBotConfig> telegramBotConfig)
    {
        Logger = logger;
        BotClient = botClient;
        _telegramBotConfig = telegramBotConfig.Value;
    }

    private ILogger Logger { get; }

    private ITelegramBotClient BotClient { get; }

    public async Task<string?> GetFileIdAsync(int messageId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(messageId);

        try
        {
            var message = await BotClient.ForwardMessageAsync(
                chatId: _telegramBotConfig.ForwardingChatId,
                fromChatId: _telegramBotConfig.VideoChatId,
                messageId);

            return message.Video?.FileId;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get file id for message {MessageId}", messageId);
            return null;
        }
    }
}
using Bounan.Bot.BusinessLogic.Clients.Shikimori;
using Bounan.Bot.BusinessLogic.CommandDto;
using Bounan.Bot.BusinessLogic.Configs;
using Bounan.Bot.BusinessLogic.Helpers;
using Bounan.Bot.TelegramBot.Telegram;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bounan.Bot.BusinessLogic.Handlers.MessageHandlers;

public class InfoMessageHandler(
    ILogger<InfoMessageHandler> logger,
    ITelegramBotClient botClient,
    IShikimoriApi shikimoriApi,
    IOptions<ShikimoriConfig> shikimoriConfig) : IMessageHandler
{
    public static bool CanHandle(Message message) => message.Text?.StartsWith(InfoCommandDto.Command) ?? false;

    public async Task HandleAsync(Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Text);

        logger.LogInformation("Handling info message {Text}", message.Text);

        var commandDto = CommandConvert.DeserializeCommand<InfoCommandDto>(message.Text);
        if (commandDto is null)
        {
            logger.LogWarning("Failed to deserialize command {Command}", message.Text);
            return;
        }

        var chatId = message.Chat.Id;

        var animeInfo = await shikimoriApi.GetAnimeInfoAsync(commandDto.MyAnimeListId, cancellationToken);
        if (animeInfo is null)
        {
            logger.LogWarning("Anime not found {MyAnimeListId}", commandDto.MyAnimeListId);
            await botClient.SendTextMessageAsync(chatId, "Аниме не найдено", cancellationToken: cancellationToken);
            return;
        }

        logger.LogInformation("Got anime info for {MyAnimeListId}: {AnimeInfo}", commandDto.MyAnimeListId, animeInfo);
        await botClient.SendPhotoAsync(
            chatId,
            new InputFileUrl(new Uri(shikimoriConfig.Value.BaseUrl, animeInfo.Image.Preview)),
            caption: animeInfo.Russian ?? animeInfo.Name,
            replyMarkup: new InlineKeyboardMarkup([
                new InlineKeyboardButton("Смотреть")
                {
                    SwitchInlineQueryCurrentChat = CommandConvert.SerializeCommand(
                        new DubsCommandDto { MyAnimeListId = commandDto.MyAnimeListId }),
                },
                new InlineKeyboardButton("Франшиза")
                {
                    SwitchInlineQueryCurrentChat = CommandConvert.SerializeCommand(
                        new RelatedCommandDto { MyAnimeListId = commandDto.MyAnimeListId }),
                },
            ]),
            cancellationToken: cancellationToken);
    }
}
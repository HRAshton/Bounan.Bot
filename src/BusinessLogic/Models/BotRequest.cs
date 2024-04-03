using Bounan.Common.Models;

namespace Bounan.Bot.BusinessLogic.Models;

/// <summary>
/// Request from the Bot to the AniMan.
/// </summary>
public record BotRequest(int MyAnimeListId, string Dub, int Episode, long ChatId) : IBotRequest;
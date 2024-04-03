using Bounan.Common.Models;

namespace Bounan.Bot.BusinessLogic.Models;

/// <summary>
/// Notification from the AniMan to the Bot.
/// </summary>
public record BotNotification(ICollection<long> ChatIds, int MyAnimeListId, string Dub, int Episode, string? FileId)
    : IBotNotification;
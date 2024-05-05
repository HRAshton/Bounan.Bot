using Bounan.Common.Models;
using Bounan.Common.Models.Notifications;

namespace Bounan.Bot.BusinessLogic.Models;

/// <summary>
/// Notification from the AniMan to the Bot.
/// </summary>
public record VideoDownloadedNotification(
    int MyAnimeListId,
    string Dub,
    int Episode,
    int? MessageId,
    ICollection<long> ChatIds,
    Scenes? Scenes)
    : IVideoDownloadedNotification;
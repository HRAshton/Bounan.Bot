using Bounan.Common.Enums;
using Bounan.Common.Models;

namespace Bounan.Bot.BusinessLogic.Models;

/// <summary>
/// Response from the AniMan to the Bot.
/// Describes the video to return to the user.
/// </summary>
public record BotResponse(VideoStatus Status, int? MessageId, Scenes? Scenes) : IBotResponse;
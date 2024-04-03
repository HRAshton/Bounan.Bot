using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Bounan.Bot.BusinessLogic.Clients.Shikimori.Models;

public record AnimeInfo
{
    public required int Id { get; [UsedImplicitly] init; }

    public required string Name { get; [UsedImplicitly] init; }

    public required Uri Url { get; [UsedImplicitly] init; }

    public string? Russian { get; [UsedImplicitly] init; }

    public int? Episodes { get; [UsedImplicitly] init; }

    [JsonPropertyName("aired_on")]
    public string? AiredOn { get; [UsedImplicitly] init; }

    public required AnimeImage Image { get; [UsedImplicitly] init; }
}
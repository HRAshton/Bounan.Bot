using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Bounan.Bot.BusinessLogic.Clients.Shikimori.Models;

public record RelatedItem
{
    public string? Relation { get; [UsedImplicitly] init; }

    [JsonPropertyName("relation_russian")]
    public string? RelationRussian { get; [UsedImplicitly] init; }

    public AnimeInfo? Anime { get; [UsedImplicitly] init; }
}
using Bounan.Bot.BusinessLogic.Clients.Shikimori.Models;
using Refit;

namespace Bounan.Bot.BusinessLogic.Clients.Shikimori;

public interface IShikimoriApi
{
    [Get("/api/animes?limit=10&censored=false&search={query}")]
    Task<AnimeInfo[]> SearchAnimeAsync(string query, CancellationToken cancellationToken = default);

    [Get("/api/animes/{id}")]
    Task<AnimeInfo?> GetAnimeInfoAsync(int id, CancellationToken cancellationToken = default);

    [Get("/api/animes/{id}/related")]
    Task<RelatedItem[]> GetRelatedAnimeAsync(int id, CancellationToken cancellationToken = default);
}
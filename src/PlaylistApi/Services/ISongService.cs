using PlaylistApi.DTOs;

namespace PlaylistApi.Services;

/// <summary>
/// Business logic contract ("ICourseService" in the lecture's class diagram).
/// Controllers only talk to this interface; it is injected via Dependency Injection.
/// </summary>
public interface ISongService
{
    /// <summary>Search + filter + sort + paginate, with a summary of the matching songs.</summary>
    PagedSongsResponse Query(SongQueryParameters query);

    /// <summary>Distinct genres in the playlist (for the "All Genres" dropdown).</summary>
    IReadOnlyList<string> GetGenres();

    SongDto? GetById(int id);
    SongDto Create(CreateSongDto dto);

    /// <summary>Returns null when the song does not exist.</summary>
    SongDto? Update(int id, UpdateSongDto dto);

    /// <summary>Returns false when the song does not exist.</summary>
    bool Delete(int id);

    /// <summary>Marks / unmarks a song as favorite. Returns null when the song does not exist.</summary>
    SongDto? SetFavorite(int id, bool isFavorite);

    /// <summary>Moves a song to a new 1-based position; the other songs shift accordingly.</summary>
    MoveResult Move(int id, int newPosition, out SongDto? song);
}

public enum MoveResult
{
    Moved,
    NotFound,
    InvalidPosition
}

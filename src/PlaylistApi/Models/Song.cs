namespace PlaylistApi.Models;

/// <summary>
/// Domain model (entity) that is kept in the in-memory data store.
/// It is never returned to the client directly – controllers work with DTOs.
/// </summary>
public class Song
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Artist { get; set; } = string.Empty;

    public string? Album { get; set; }

    /// <summary>Song length in seconds (e.g. 212 = 3:32).</summary>
    public int DurationSeconds { get; set; }

    public string Genre { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public bool IsFavorite { get; set; }

    /// <summary>Order of the song inside the playlist (1 = first). Changed by drag &amp; drop.</summary>
    public int Position { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

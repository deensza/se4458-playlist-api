using System.ComponentModel.DataAnnotations;

namespace PlaylistApi.DTOs;

/// <summary>What the API returns for a song.</summary>
public class SongDto
{
    /// <example>1</example>
    public int Id { get; set; }

    /// <example>Blinding Lights</example>
    public string Title { get; set; } = string.Empty;

    /// <example>The Weeknd</example>
    public string Artist { get; set; } = string.Empty;

    /// <example>After Hours</example>
    public string? Album { get; set; }

    /// <summary>Song length in seconds.</summary>
    /// <example>212</example>
    public int DurationSeconds { get; set; }

    /// <summary>Human readable duration.</summary>
    /// <example>3:32</example>
    public string Duration { get; set; } = string.Empty;

    /// <example>Pop</example>
    public string Genre { get; set; } = string.Empty;

    /// <example>https://example.com/covers/after-hours.jpg</example>
    public string? CoverImageUrl { get; set; }

    /// <example>true</example>
    public bool IsFavorite { get; set; }

    /// <summary>Order of the song in the playlist (1 = first).</summary>
    /// <example>1</example>
    public int Position { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Request body for adding a new song (POST).</summary>
public class CreateSongDto
{
    /// <summary>Song title (required, 1 – 200 characters).</summary>
    /// <example>Flowers</example>
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Artist name (required, 1 – 200 characters).</summary>
    /// <example>Miley Cyrus</example>
    [Required, StringLength(200, MinimumLength = 1)]
    public string Artist { get; set; } = string.Empty;

    /// <summary>Album name (optional).</summary>
    /// <example>Endless Summer Vacation</example>
    [StringLength(200)]
    public string? Album { get; set; }

    /// <summary>Song length in seconds (1 – 3600).</summary>
    /// <example>200</example>
    [Range(1, 3600)]
    public int DurationSeconds { get; set; }

    /// <summary>Genre (required), e.g. Pop, R&amp;B, Rock.</summary>
    /// <example>Pop</example>
    [Required, StringLength(50, MinimumLength = 1)]
    public string Genre { get; set; } = string.Empty;

    /// <summary>Album cover image URL (optional).</summary>
    /// <example>https://example.com/covers/flowers.jpg</example>
    [Url]
    public string? CoverImageUrl { get; set; }

    /// <summary>Add the song to favorites right away.</summary>
    /// <example>false</example>
    public bool IsFavorite { get; set; }
}

/// <summary>
/// Request body for editing a song (PUT). PUT replaces all editable fields,
/// which keeps the operation idempotent.
/// </summary>
public class UpdateSongDto
{
    /// <summary>Song title (required, 1 – 200 characters).</summary>
    /// <example>Levitating (feat. DaBaby)</example>
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Artist name (required, 1 – 200 characters).</summary>
    /// <example>Dua Lipa</example>
    [Required, StringLength(200, MinimumLength = 1)]
    public string Artist { get; set; } = string.Empty;

    /// <summary>Album name (optional).</summary>
    /// <example>Future Nostalgia</example>
    [StringLength(200)]
    public string? Album { get; set; }

    /// <summary>Song length in seconds (1 – 3600).</summary>
    /// <example>203</example>
    [Range(1, 3600)]
    public int DurationSeconds { get; set; }

    /// <summary>Genre (required).</summary>
    /// <example>Pop</example>
    [Required, StringLength(50, MinimumLength = 1)]
    public string Genre { get; set; } = string.Empty;

    /// <summary>Album cover image URL (optional).</summary>
    /// <example>https://example.com/covers/future-nostalgia.jpg</example>
    [Url]
    public string? CoverImageUrl { get; set; }

    /// <example>true</example>
    public bool IsFavorite { get; set; }
}

/// <summary>Mapping helpers between the entity and the DTOs.</summary>
public static class SongMappings
{
    public static SongDto ToDto(this Models.Song song) => new()
    {
        Id = song.Id,
        Title = song.Title,
        Artist = song.Artist,
        Album = song.Album,
        DurationSeconds = song.DurationSeconds,
        Duration = FormatDuration(song.DurationSeconds),
        Genre = song.Genre,
        CoverImageUrl = song.CoverImageUrl,
        IsFavorite = song.IsFavorite,
        Position = song.Position,
        CreatedAt = song.CreatedAt,
        UpdatedAt = song.UpdatedAt
    };

    /// <summary>212 -> "3:32"</summary>
    public static string FormatDuration(int totalSeconds) =>
        $"{totalSeconds / 60}:{totalSeconds % 60:D2}";

    /// <summary>1046 -> "17 min 26 sec", 3725 -> "1 hr 2 min 5 sec"</summary>
    public static string FormatTotalDuration(int totalSeconds)
    {
        var hours = totalSeconds / 3600;
        var minutes = totalSeconds % 3600 / 60;
        var seconds = totalSeconds % 60;
        return hours > 0
            ? $"{hours} hr {minutes} min {seconds} sec"
            : $"{minutes} min {seconds} sec";
    }
}

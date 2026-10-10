using System.ComponentModel.DataAnnotations;

namespace PlaylistApi.DTOs;

/// <summary>Fields a playlist can be sorted by ("Sort by" dropdown and sortable columns on the screen).</summary>
public enum SongSortField
{
    Position,
    Title,
    Artist,
    Album,
    Duration,
    Genre,
    CreatedAt
}

public enum SortOrder
{
    Asc,
    Desc
}

/// <summary>
/// Query string parameters for GET /api/v1/songs.
/// Example: /api/v1/songs?search=weeknd&amp;genre=Pop&amp;sortBy=Title&amp;sortOrder=Asc&amp;page=1&amp;pageSize=10
/// </summary>
public class SongQueryParameters
{
    /// <summary>Searches in title, artist and album (case-insensitive).</summary>
    [StringLength(100)]
    public string? Search { get; set; }

    /// <summary>Only songs of this genre (e.g. "Pop", "R&amp;B"). Empty = all genres.</summary>
    [StringLength(50)]
    public string? Genre { get; set; }

    /// <summary>true = only favorites, false = only non-favorites, empty = all.</summary>
    public bool? IsFavorite { get; set; }

    /// <summary>Default: Position (the playlist order).</summary>
    public SongSortField SortBy { get; set; } = SongSortField.Position;

    public SortOrder SortOrder { get; set; } = SortOrder.Asc;

    /// <summary>Page number, starting from 1.</summary>
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    /// <summary>Items per page (1 – 100).</summary>
    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}

/// <summary>Request body for moving a song to a new place in the playlist (drag &amp; drop).</summary>
public class MoveSongDto
{
    /// <summary>New 1-based position (1 = top of the playlist).</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue)]
    public int Position { get; set; }
}

/// <summary>"5 songs • 17 min 26 sec" header of the playlist screen.</summary>
public class PlaylistSummaryDto
{
    /// <example>5</example>
    public int SongCount { get; set; }

    /// <example>1046</example>
    public int TotalDurationSeconds { get; set; }

    /// <summary>Human readable total.</summary>
    /// <example>17 min 26 sec</example>
    public string TotalDuration { get; set; } = string.Empty;
}

/// <summary>Paged list response (lecture: "API Pagination").</summary>
public class PagedSongsResponse
{
    public IReadOnlyList<SongDto> Items { get; set; } = Array.Empty<SongDto>();
    public int Page { get; set; }
    public int PageSize { get; set; }

    /// <summary>Number of songs that match the filters (all pages).</summary>
    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    /// <summary>Count and total duration of the songs that match the filters (all pages).</summary>
    public PlaylistSummaryDto Summary { get; set; } = new();
}

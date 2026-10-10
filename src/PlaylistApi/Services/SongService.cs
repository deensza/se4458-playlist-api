using PlaylistApi.Data;
using PlaylistApi.DTOs;
using PlaylistApi.Models;

namespace PlaylistApi.Services;

/// <summary>
/// Business logic for the playlist ("CourseService" in the lecture's class diagram).
/// Converts DTOs to entities, cleans the input and uses the repository for storage.
/// </summary>
public class SongService : ISongService
{
    private readonly ISongRepository _repository;

    // The repository is given by the DI container – the service never creates it with "new".
    public SongService(ISongRepository repository)
    {
        _repository = repository;
    }

    public PagedSongsResponse Query(SongQueryParameters query)
    {
        IEnumerable<Song> songs = _repository.GetAll();

        // 1) Search in title, artist and album ("Search songs, artist, or album...")
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            songs = songs.Where(s =>
                Contains(s.Title, term) || Contains(s.Artist, term) || Contains(s.Album, term));
        }

        // 2) Filter by genre ("All Genres" dropdown)
        if (!string.IsNullOrWhiteSpace(query.Genre))
        {
            var genre = query.Genre.Trim();
            songs = songs.Where(s => string.Equals(s.Genre, genre, StringComparison.OrdinalIgnoreCase));
        }

        // 3) Filter by favorite (star)
        if (query.IsFavorite.HasValue)
        {
            songs = songs.Where(s => s.IsFavorite == query.IsFavorite.Value);
        }

        // 4) Sort ("Sort by" dropdown); position is the tie-breaker so the result is always stable.
        var sorted = Sort(songs, query.SortBy, query.SortOrder).ToList();

        // 5) Summary of all matching songs ("5 songs • 17 min 26 sec")
        var totalSeconds = sorted.Sum(s => s.DurationSeconds);
        var summary = new PlaylistSummaryDto
        {
            SongCount = sorted.Count,
            TotalDurationSeconds = totalSeconds,
            TotalDuration = SongMappings.FormatTotalDuration(totalSeconds)
        };

        // 6) Pagination
        var items = sorted
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(s => s.ToDto())
            .ToList();

        return new PagedSongsResponse
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = sorted.Count,
            TotalPages = (int)Math.Ceiling(sorted.Count / (double)query.PageSize),
            Summary = summary
        };
    }

    public IReadOnlyList<string> GetGenres() =>
        _repository.GetAll()
            .Select(s => s.Genre)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g, StringComparer.OrdinalIgnoreCase)
            .ToList();

    public SongDto? GetById(int id) =>
        _repository.GetById(id)?.ToDto();

    public SongDto Create(CreateSongDto dto)
    {
        var song = new Song
        {
            Title = dto.Title.Trim(),
            Artist = dto.Artist.Trim(),
            Album = Clean(dto.Album),
            DurationSeconds = dto.DurationSeconds,
            Genre = dto.Genre.Trim(),
            CoverImageUrl = Clean(dto.CoverImageUrl),
            IsFavorite = dto.IsFavorite
        };

        return _repository.Add(song).ToDto();
    }

    public SongDto? Update(int id, UpdateSongDto dto)
    {
        var existing = _repository.GetById(id);
        if (existing is null) return null;

        existing.Title = dto.Title.Trim();
        existing.Artist = dto.Artist.Trim();
        existing.Album = Clean(dto.Album);
        existing.DurationSeconds = dto.DurationSeconds;
        existing.Genre = dto.Genre.Trim();
        existing.CoverImageUrl = Clean(dto.CoverImageUrl);
        existing.IsFavorite = dto.IsFavorite;

        _repository.Update(existing);
        return _repository.GetById(id)!.ToDto();
    }

    public bool Delete(int id) => _repository.Delete(id);

    public SongDto? SetFavorite(int id, bool isFavorite)
    {
        var existing = _repository.GetById(id);
        if (existing is null) return null;

        existing.IsFavorite = isFavorite;
        _repository.Update(existing);
        return _repository.GetById(id)!.ToDto();
    }

    public MoveResult Move(int id, int newPosition, out SongDto? song)
    {
        song = null;
        var ordered = _repository.GetAll().ToList();   // already sorted by position

        var moving = ordered.FirstOrDefault(s => s.Id == id);
        if (moving is null) return MoveResult.NotFound;
        if (newPosition < 1 || newPosition > ordered.Count) return MoveResult.InvalidPosition;

        // Take the song out and insert it at the new place – exactly what drag & drop does.
        ordered.Remove(moving);
        ordered.Insert(newPosition - 1, moving);
        _repository.SetPositions(ordered.Select(s => s.Id).ToList());

        song = _repository.GetById(id)!.ToDto();
        return MoveResult.Moved;
    }

    private static IEnumerable<Song> Sort(IEnumerable<Song> songs, SongSortField field, SortOrder order)
    {
        var comparer = StringComparer.OrdinalIgnoreCase;
        var desc = order == SortOrder.Desc;

        IOrderedEnumerable<Song> ordered = field switch
        {
            SongSortField.Title     => desc ? songs.OrderByDescending(s => s.Title, comparer)       : songs.OrderBy(s => s.Title, comparer),
            SongSortField.Artist    => desc ? songs.OrderByDescending(s => s.Artist, comparer)      : songs.OrderBy(s => s.Artist, comparer),
            SongSortField.Album     => desc ? songs.OrderByDescending(s => s.Album ?? "", comparer) : songs.OrderBy(s => s.Album ?? "", comparer),
            SongSortField.Duration  => desc ? songs.OrderByDescending(s => s.DurationSeconds)       : songs.OrderBy(s => s.DurationSeconds),
            SongSortField.Genre     => desc ? songs.OrderByDescending(s => s.Genre, comparer)       : songs.OrderBy(s => s.Genre, comparer),
            SongSortField.CreatedAt => desc ? songs.OrderByDescending(s => s.CreatedAt)             : songs.OrderBy(s => s.CreatedAt),
            _                       => desc ? songs.OrderByDescending(s => s.Position)              : songs.OrderBy(s => s.Position)
        };

        return ordered.ThenBy(s => s.Position);
    }

    private static bool Contains(string? value, string term) =>
        value is not null && value.Contains(term, StringComparison.OrdinalIgnoreCase);

    /// <summary>Turns empty / whitespace strings into null and trims the rest.</summary>
    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

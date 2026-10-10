using PlaylistApi.Models;

namespace PlaylistApi.Data;

/// <summary>
/// In-memory data store. All data lives in a List inside the process,
/// so it is reset every time the application restarts.
/// Registered as a Singleton, therefore a lock is used to keep it thread-safe
/// when several HTTP requests arrive at the same time.
/// </summary>
public class InMemorySongRepository : ISongRepository
{
    private readonly List<Song> _songs = new();
    private readonly object _lock = new();
    private int _nextId = 1;

    public InMemorySongRepository()
    {
        Seed();
    }

    public IReadOnlyList<Song> GetAll()
    {
        lock (_lock)
        {
            // Return copies so callers cannot change the store without going through the repository.
            return _songs.OrderBy(s => s.Position).Select(Clone).ToList();
        }
    }

    public Song? GetById(int id)
    {
        lock (_lock)
        {
            var song = _songs.FirstOrDefault(s => s.Id == id);
            return song is null ? null : Clone(song);
        }
    }

    public Song Add(Song song)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            song.Id = _nextId++;
            song.Position = _songs.Count + 1;   // new songs go to the end of the playlist
            song.CreatedAt = now;
            song.UpdatedAt = now;
            _songs.Add(Clone(song));
            return Clone(song);
        }
    }

    public bool Update(Song song)
    {
        lock (_lock)
        {
            var index = _songs.FindIndex(s => s.Id == song.Id);
            if (index < 0) return false;

            var existing = _songs[index];
            song.CreatedAt = existing.CreatedAt;
            song.Position = existing.Position;
            song.UpdatedAt = DateTime.UtcNow;
            _songs[index] = Clone(song);
            return true;
        }
    }

    public bool Delete(int id)
    {
        lock (_lock)
        {
            var removed = _songs.RemoveAll(s => s.Id == id) > 0;
            if (removed) Renumber(_songs.OrderBy(s => s.Position).ToList());
            return removed;
        }
    }

    public void SetPositions(IReadOnlyList<int> orderedIds)
    {
        lock (_lock)
        {
            // Ids that no longer exist are skipped (e.g. deleted by another request in the meantime),
            // and songs missing from the list keep their relative order at the end.
            var ordered = orderedIds
                .Distinct()
                .Select(id => _songs.FirstOrDefault(s => s.Id == id))
                .OfType<Song>()
                .ToList();
            ordered.AddRange(_songs.Where(s => !ordered.Contains(s)).OrderBy(s => s.Position));
            Renumber(ordered);
        }
    }

    /// <summary>Gives positions 1..n without gaps, in the given order.</summary>
    private static void Renumber(List<Song> ordered)
    {
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].Position = i + 1;
        }
    }

    private static Song Clone(Song s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        Artist = s.Artist,
        Album = s.Album,
        DurationSeconds = s.DurationSeconds,
        Genre = s.Genre,
        CoverImageUrl = s.CoverImageUrl,
        IsFavorite = s.IsFavorite,
        Position = s.Position,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };

    /// <summary>Sample data taken from the playlist screen in the assignment (5 songs, 17 min 26 sec).</summary>
    private void Seed()
    {
        var samples = new[]
        {
            new Song { Title = "Blinding Lights",  Artist = "The Weeknd",   Album = "After Hours",       DurationSeconds = 212, Genre = "Pop", IsFavorite = true  },
            new Song { Title = "Levitating",       Artist = "Dua Lipa",     Album = "Future Nostalgia",  DurationSeconds = 220, Genre = "Pop", IsFavorite = false },
            new Song { Title = "Watermelon Sugar", Artist = "Harry Styles", Album = "Fine Line",         DurationSeconds = 214, Genre = "Pop", IsFavorite = true  },
            new Song { Title = "Save Your Tears",  Artist = "The Weeknd",   Album = "After Hours",       DurationSeconds = 246, Genre = "R&B", IsFavorite = false },
            new Song { Title = "Peaches",          Artist = "Justin Bieber",Album = "Justice",           DurationSeconds = 154, Genre = "Pop", IsFavorite = true  }
        };

        foreach (var song in samples)
        {
            Add(song);
        }
    }
}

using PlaylistApi.Models;

namespace PlaylistApi.Data;

/// <summary>
/// Data access contract ("CourseAccess" in the lecture's class diagram).
/// The service layer depends on this interface, so the in-memory store
/// could be swapped for a real database later without touching the service.
/// </summary>
public interface ISongRepository
{
    IReadOnlyList<Song> GetAll();
    Song? GetById(int id);
    Song Add(Song song);
    bool Update(Song song);
    bool Delete(int id);

    /// <summary>Replaces the order of all songs (used by drag &amp; drop reorder).</summary>
    void SetPositions(IReadOnlyList<int> orderedIds);
}

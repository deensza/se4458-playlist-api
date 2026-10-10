using Microsoft.AspNetCore.Mvc;
using PlaylistApi.DTOs;
using PlaylistApi.Services;

namespace PlaylistApi.Controllers;

/// <summary>
/// REST endpoints for the songs in the playlist ("CourseControllerWebAPI" in the lecture's class diagram).
/// Resource name is plural ("songs") and versioned ("v1").
/// </summary>
[ApiController]
[Route("api/v1/songs")]
[Produces("application/json")]
public class SongsController : ControllerBase
{
    private readonly ISongService _songService;

    public SongsController(ISongService songService)
    {
        _songService = songService;
    }

    /// <summary>
    /// Lists songs with optional search, genre / favorite filter, sorting and pagination.
    /// The response also contains a summary ("5 songs • 17 min 26 sec") of all matching songs.
    /// </summary>
    /// <remarks>
    /// Examples:
    /// - /api/v1/songs                                  → whole playlist in order
    /// - /api/v1/songs?search=weeknd                    → title, artist or album contains "weeknd"
    /// - /api/v1/songs?genre=Pop&amp;sortBy=Duration&amp;sortOrder=Desc
    /// - /api/v1/songs?isFavorite=true&amp;page=1&amp;pageSize=2
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedSongsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<PagedSongsResponse> GetSongs([FromQuery] SongQueryParameters query)
    {
        return Ok(_songService.Query(query));
    }

    /// <summary>Lists the distinct genres in the playlist (for the "All Genres" filter).</summary>
    [HttpGet("genres")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<string>> GetGenres()
    {
        return Ok(_songService.GetGenres());
    }

    /// <summary>Gets a single song by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SongDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<SongDto> GetById(int id)
    {
        var song = _songService.GetById(id);
        return song is null ? SongNotFound(id) : Ok(song);
    }

    /// <summary>Adds a new song to the end of the playlist.</summary>
    /// <response code="201">Song created. The Location header points to the new song.</response>
    /// <response code="400">Validation error (e.g. missing title, duration out of range).</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(SongDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<SongDto> Create([FromBody] CreateSongDto dto)
    {
        // [ApiController] already returns 400 automatically when the DTO validation fails.
        var created = _songService.Create(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Edits a song. PUT replaces all editable fields (idempotent).</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(SongDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<SongDto> Update(int id, [FromBody] UpdateSongDto dto)
    {
        var updated = _songService.Update(id, dto);
        return updated is null ? SongNotFound(id) : Ok(updated);
    }

    /// <summary>Removes a song from the playlist.</summary>
    /// <response code="204">Song removed.</response>
    /// <response code="404">No song with this id.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        return _songService.Delete(id) ? NoContent() : SongNotFound(id);
    }

    /// <summary>Marks a song as favorite (filled star). Idempotent – calling it twice changes nothing.</summary>
    [HttpPut("{id:int}/favorite")]
    [ProducesResponseType(typeof(SongDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<SongDto> AddToFavorites(int id)
    {
        var song = _songService.SetFavorite(id, true);
        return song is null ? SongNotFound(id) : Ok(song);
    }

    /// <summary>Removes a song from favorites (empty star). Idempotent.</summary>
    [HttpDelete("{id:int}/favorite")]
    [ProducesResponseType(typeof(SongDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<SongDto> RemoveFromFavorites(int id)
    {
        var song = _songService.SetFavorite(id, false);
        return song is null ? SongNotFound(id) : Ok(song);
    }

    /// <summary>
    /// Moves a song to a new position in the playlist (drag &amp; drop handle on the screen).
    /// The other songs shift up / down. Idempotent – same request, same order.
    /// </summary>
    /// <response code="200">Song moved; returns the song with its new position.</response>
    /// <response code="400">Position is smaller than 1 or bigger than the number of songs.</response>
    /// <response code="404">No song with this id.</response>
    [HttpPut("{id:int}/position")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(SongDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<SongDto> Move(int id, [FromBody] MoveSongDto dto)
    {
        return _songService.Move(id, dto.Position, out var song) switch
        {
            MoveResult.Moved => Ok(song),
            MoveResult.NotFound => SongNotFound(id),
            _ => Problem(statusCode: StatusCodes.Status400BadRequest,
                         title: "Invalid position",
                         detail: $"Position must be between 1 and the number of songs in the playlist.")
        };
    }

    private ObjectResult SongNotFound(int id) =>
        Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Song not found",
                detail: $"No song exists with id {id}.");
}

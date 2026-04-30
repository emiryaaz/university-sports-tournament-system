using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsTournament.Api.Data;
using SportsTournament.Api.DTOs;
using SportsTournament.Api.Models;

namespace SportsTournament.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FixturesController : ControllerBase
{
    private readonly AppDbContext _context;

    public FixturesController(AppDbContext context)
    {
        _context = context;
    }
[HttpGet]
public async Task<ActionResult<IEnumerable<FixtureResponseDto>>> GetFixtures()
{
    var fixtures = await _context.Fixtures
        .Include(f => f.Tournament)
        .Include(f => f.HomeTeam)
        .Include(f => f.AwayTeam)
        .Select(f => new FixtureResponseDto
        {
            Id = f.Id,
            TournamentId = f.TournamentId,
            TournamentName = f.Tournament.Name,
            HomeTeamId = f.HomeTeamId,
            HomeTeamName = f.HomeTeam.Name,
            AwayTeamId = f.AwayTeamId,
            AwayTeamName = f.AwayTeam.Name,
            MatchDate = f.MatchDate,
            Status = f.Status
        })
        .ToListAsync();

    return fixtures;
}

[HttpGet("{id}")]
public async Task<ActionResult<FixtureResponseDto>> GetFixture(int id)
{
    var fixture = await _context.Fixtures
        .Include(f => f.Tournament)
        .Include(f => f.HomeTeam)
        .Include(f => f.AwayTeam)
        .Where(f => f.Id == id)
        .Select(f => new FixtureResponseDto
        {
            Id = f.Id,
            TournamentId = f.TournamentId,
            TournamentName = f.Tournament.Name,
            HomeTeamId = f.HomeTeamId,
            HomeTeamName = f.HomeTeam.Name,
            AwayTeamId = f.AwayTeamId,
            AwayTeamName = f.AwayTeam.Name,
            MatchDate = f.MatchDate,
            Status = f.Status
        })
        .FirstOrDefaultAsync();

    if (fixture == null)
        return NotFound();

    return fixture;
}

[HttpGet("tournament/{tournamentId}")]
public async Task<ActionResult<IEnumerable<FixtureResponseDto>>> GetFixturesByTournament(int tournamentId)
{
    var fixtures = await _context.Fixtures
        .Where(f => f.TournamentId == tournamentId)
        .Include(f => f.Tournament)
        .Include(f => f.HomeTeam)
        .Include(f => f.AwayTeam)
        .Select(f => new FixtureResponseDto
        {
            Id = f.Id,
            TournamentId = f.TournamentId,
            TournamentName = f.Tournament.Name,
            HomeTeamId = f.HomeTeamId,
            HomeTeamName = f.HomeTeam.Name,
            AwayTeamId = f.AwayTeamId,
            AwayTeamName = f.AwayTeam.Name,
            MatchDate = f.MatchDate,
            Status = f.Status
        })
        .ToListAsync();

    return fixtures;
}

[HttpGet("{id}/result")]
public async Task<ActionResult<MatchResultResponseDto>> GetFixtureResult(int id)
{
    var result = await _context.MatchResults
        .Include(r => r.Fixture)
        .ThenInclude(f => f.Tournament)
        .Include(r => r.Fixture)
        .ThenInclude(f => f.HomeTeam)
        .Include(r => r.Fixture)
        .ThenInclude(f => f.AwayTeam)
        .Include(r => r.WinnerTeam)
        .Where(r => r.FixtureId == id)
        .Select(r => new MatchResultResponseDto
        {
            FixtureId = r.FixtureId,
            TournamentName = r.Fixture.Tournament.Name,
            HomeTeamName = r.Fixture.HomeTeam.Name,
            HomeScore = r.HomeScore,
            AwayTeamName = r.Fixture.AwayTeam.Name,
            AwayScore = r.AwayScore,
            WinnerTeamName = r.WinnerTeam.Name
        })
        .FirstOrDefaultAsync();

    if (result == null)
        return NotFound("Result not found for this fixture.");

    return result;
}

    [HttpPost("enter-result")]
    public async Task<IActionResult> EnterResult(EnterMatchResultDto dto)
    {
        var fixture = await _context.Fixtures
            .Include(f => f.Tournament)
            .FirstOrDefaultAsync(f => f.Id == dto.FixtureId);

        if (fixture == null)
            return NotFound("Fixture not found.");

        if (fixture.Status == "Completed")
            return BadRequest("Match already completed.");

        if (dto.HomeScore < 0 || dto.AwayScore < 0)
            return BadRequest("Scores cannot be negative.");

        int winnerId;

        if (dto.HomeScore > dto.AwayScore)
            winnerId = fixture.HomeTeamId;
        else if (dto.AwayScore > dto.HomeScore)
            winnerId = fixture.AwayTeamId;
        else
            winnerId = 0; // draw

        var result = new MatchResult
        {
            FixtureId = fixture.Id,
            HomeScore = dto.HomeScore,
            AwayScore = dto.AwayScore,
            WinnerTeamId = winnerId == 0 ? fixture.HomeTeamId : winnerId
        };

        fixture.Status = "Completed";

        _context.MatchResults.Add(result);

        //  Standing update
await UpdateStandings(fixture, dto.HomeScore, dto.AwayScore);

await _context.SaveChangesAsync();

var allFixturesCompleted = await _context.Fixtures
    .Where(f => f.TournamentId == fixture.TournamentId)
    .AllAsync(f => f.Status == "Completed");

if (allFixturesCompleted)
{
    var tournament = await _context.Tournaments.FindAsync(fixture.TournamentId);

    if(tournament != null)
    {
        tournament.Status = "Finished";
        await _context.SaveChangesAsync();
    }
}

return Ok("Result entered, standings updated, and tournament status checked.");
    }

    private async Task UpdateStandings(Fixture fixture, int homeScore, int awayScore)
    {
        var tournamentId = fixture.TournamentId;

        var homeStanding = await _context.Standings
            .FirstOrDefaultAsync(s => s.TournamentId == tournamentId && s.TeamId == fixture.HomeTeamId);

        var awayStanding = await _context.Standings
            .FirstOrDefaultAsync(s => s.TournamentId == tournamentId && s.TeamId == fixture.AwayTeamId);

        if (homeStanding == null)
        {
            homeStanding = new Standing
            {
                TournamentId = tournamentId,
                TeamId = fixture.HomeTeamId
            };
            _context.Standings.Add(homeStanding);
        }

        if (awayStanding == null)
        {
            awayStanding = new Standing
            {
                TournamentId = tournamentId,
                TeamId = fixture.AwayTeamId
            };
            _context.Standings.Add(awayStanding);
        }

        homeStanding.Played++;
        awayStanding.Played++;

        if (homeScore > awayScore)
        {
            homeStanding.Wins++;
            homeStanding.Points += 3;
            awayStanding.Losses++;
        }
        else if (awayScore > homeScore)
        {
            awayStanding.Wins++;
            awayStanding.Points += 3;
            homeStanding.Losses++;
        }
        else
        {
            homeStanding.Draws++;
            awayStanding.Draws++;
            homeStanding.Points += 1;
            awayStanding.Points += 1;
        }
    }
}

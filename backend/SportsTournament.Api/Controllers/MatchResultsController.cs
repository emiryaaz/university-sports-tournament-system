using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsTournament.Api.Data;
using SportsTournament.Api.Models;

namespace SportsTournament.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchResultsController : ControllerBase
{
    private readonly AppDbContext _context;

    public MatchResultsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> EnterMatchResult(CreateMatchResultDto dto)
    {
        if (dto.HomeScore < 0 || dto.AwayScore < 0)
            return BadRequest("Scores must be non-negative.");

        var fixture = await _context.Fixtures
            .Include(f => f.Tournament)
            .Include(f => f.MatchResult)
            .FirstOrDefaultAsync(f => f.Id == dto.FixtureId);

        if (fixture == null)
            return NotFound("Fixture not found.");

        if (fixture.MatchResult != null || fixture.Status == "Completed")
            return BadRequest("This fixture already has a result.");

        var isDraw = dto.HomeScore == dto.AwayScore;

        if (fixture.Tournament.Format == "SingleElimination" && isDraw)
            return BadRequest("Draws are not allowed in Single Elimination tournaments.");

        int? winnerTeamId = null;

        if (dto.HomeScore > dto.AwayScore)
            winnerTeamId = fixture.HomeTeamId;
        else if (dto.AwayScore > dto.HomeScore)
            winnerTeamId = fixture.AwayTeamId;

        var result = new MatchResult
        {
            FixtureId = fixture.Id,
            HomeScore = dto.HomeScore,
            AwayScore = dto.AwayScore,
            WinnerTeamId = winnerTeamId
        };

        fixture.Status = "Completed";

        _context.MatchResults.Add(result);

        if (fixture.Tournament.Format == "RoundRobin")
        {
            await UpdateRoundRobinStandings(fixture, dto.HomeScore, dto.AwayScore);
        }
        else if (fixture.Tournament.Format == "SingleElimination" && winnerTeamId.HasValue)
        {
            await AdvanceWinnerToNextRound(fixture, winnerTeamId.Value);
        }

        await _context.SaveChangesAsync();

        return Ok("Match result entered successfully.");
    }

    private async Task UpdateRoundRobinStandings(Fixture fixture, int homeScore, int awayScore)
    {
        var homeStanding = await GetOrCreateStanding(fixture.TournamentId, fixture.HomeTeamId);
        var awayStanding = await GetOrCreateStanding(fixture.TournamentId, fixture.AwayTeamId);

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

    private async Task<Standing> GetOrCreateStanding(int tournamentId, int teamId)
    {
        var standing = await _context.Standings
            .FirstOrDefaultAsync(s => s.TournamentId == tournamentId && s.TeamId == teamId);

        if (standing != null)
            return standing;

        standing = new Standing
        {
            TournamentId = tournamentId,
            TeamId = teamId,
            Played = 0,
            Wins = 0,
            Draws = 0,
            Losses = 0,
            Points = 0
        };

        _context.Standings.Add(standing);

        return standing;
    }

    private async Task AdvanceWinnerToNextRound(Fixture completedFixture, int winnerTeamId)
    {
        var sameRoundFixtures = await _context.Fixtures
            .Where(f =>
                f.TournamentId == completedFixture.TournamentId &&
                f.RoundNumber == completedFixture.RoundNumber)
            .ToListAsync();

        var allRoundCompleted = sameRoundFixtures.All(f =>
            f.Id == completedFixture.Id || f.Status == "Completed");

        if (!allRoundCompleted)
            return;

        var winners = await _context.MatchResults
            .Include(mr => mr.Fixture)
            .Where(mr =>
                mr.Fixture.TournamentId == completedFixture.TournamentId &&
                mr.Fixture.RoundNumber == completedFixture.RoundNumber &&
                mr.WinnerTeamId != null)
            .Select(mr => mr.WinnerTeamId!.Value)
            .ToListAsync();

        if (!winners.Contains(winnerTeamId))
            winners.Add(winnerTeamId);

        if (winners.Count == 1)
        {
            completedFixture.Tournament.Status = "Completed";
            return;
        }

        var nextRound = completedFixture.RoundNumber + 1;
        var nextRoundExists = await _context.Fixtures.AnyAsync(f =>
            f.TournamentId == completedFixture.TournamentId &&
            f.RoundNumber == nextRound);

        if (nextRoundExists)
            return;

        var currentDate = completedFixture.MatchDate.AddDays(1);
        var matchNumber = 1;

        for (int i = 0; i < winners.Count; i += 2)
        {
            if (i + 1 >= winners.Count)
                break;

            _context.Fixtures.Add(new Fixture
            {
                TournamentId = completedFixture.TournamentId,
                HomeTeamId = winners[i],
                AwayTeamId = winners[i + 1],
                MatchDate = currentDate,
                Status = "Scheduled",
                RoundNumber = nextRound,
                MatchNumber = matchNumber
            });

            currentDate = currentDate.AddDays(1);
            matchNumber++;
        }
    }
}

public class CreateMatchResultDto
{
    public int FixtureId { get; set; }

    public int HomeScore { get; set; }

    public int AwayScore { get; set; }
}

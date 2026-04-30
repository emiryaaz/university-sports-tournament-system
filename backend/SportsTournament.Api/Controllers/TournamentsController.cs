using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsTournament.Api.Data;
using SportsTournament.Api.DTOs;
using SportsTournament.Api.Models;

namespace SportsTournament.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TournamentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TournamentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TournamentResponseDto>>> GetTournaments()
    {
        var tournaments = await _context.Tournaments
            .Include(t => t.TournamentTeams)
            .ThenInclude(tt => tt.Team)
            .Select(t => new TournamentResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                SportType = t.SportType,
                Format = t.Format,
                Status = t.Status,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                Teams = t.TournamentTeams.Select(tt => tt.Team.Name).ToList()
            })
            .ToListAsync();

        return tournaments;
    }

[HttpGet("{id}")]
public async Task<ActionResult<TournamentResponseDto>> GetTournament(int id)
{
    var tournament = await _context.Tournaments
        .Where(t => t.Id == id)
        .Include(t => t.TournamentTeams)
        .ThenInclude(tt => tt.Team)
        .Include(t => t.Fixtures)
        .ThenInclude(f => f.HomeTeam)
        .Include(t => t.Fixtures)
        .ThenInclude(f => f.AwayTeam)
        .Select(t => new TournamentResponseDto
        {
            Id = t.Id,
            Name = t.Name,
            SportType = t.SportType,
            Format = t.Format,
            Status = t.Status,
            StartDate = t.StartDate,
            EndDate = t.EndDate,
            Teams = t.TournamentTeams.Select(tt => tt.Team.Name).ToList(),
            Fixtures = t.Fixtures.Select(f => new FixtureResponseDto
            {
                Id = f.Id,
                TournamentId = f.TournamentId,
                TournamentName = t.Name,
                HomeTeamId = f.HomeTeamId,
                HomeTeamName = f.HomeTeam.Name,
                AwayTeamId = f.AwayTeamId,
                AwayTeamName = f.AwayTeam.Name,
                MatchDate = f.MatchDate,
                Status = f.Status
            }).ToList()
        })
        .FirstOrDefaultAsync();

    if (tournament == null)
        return NotFound();

    return tournament;
}
    [HttpPost]
    public async Task<ActionResult<TournamentResponseDto>> CreateTournament(CreateTournamentDto dto)
    {
        if (dto.StartDate >= dto.EndDate)
            return BadRequest("Start date must be earlier than end date.");

        if (dto.Format != "RoundRobin" && dto.Format != "SingleElimination")
            return BadRequest("Tournament format must be RoundRobin or SingleElimination.");

        var teamIds = dto.TeamIds.Distinct().ToList();

        if (teamIds.Count < 2)
            return BadRequest("A tournament must have at least 2 teams.");

        var teams = await _context.Teams
            .Where(t => teamIds.Contains(t.Id))
            .ToListAsync();

        if (teams.Count != teamIds.Count)
            return BadRequest("One or more teams were not found.");

        var invalidSportTeams = teams
            .Where(t => t.SportType != dto.SportType)
            .ToList();

        if (invalidSportTeams.Any())
            return BadRequest("All teams must have the same sport type as the tournament.");

        var tournament = new Tournament
        {
            Name = dto.Name,
            SportType = dto.SportType,
            Format = dto.Format,
            Status = "Draft",
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TournamentTeams = teamIds.Select(teamId => new TournamentTeam
            {
                TeamId = teamId
            }).ToList()
        };

        _context.Tournaments.Add(tournament);
        await _context.SaveChangesAsync();

        var response = new TournamentResponseDto
        {
            Id = tournament.Id,
            Name = tournament.Name,
            SportType = tournament.SportType,
            Format = tournament.Format,
            Status = tournament.Status,
            StartDate = tournament.StartDate,
            EndDate = tournament.EndDate,
            Teams = teams.Select(t => t.Name).ToList()
        };

        return CreatedAtAction(nameof(GetTournament), new { id = tournament.Id }, response);
    }

    [HttpPost("{id}/generate-fixtures")]
    public async Task<ActionResult<IEnumerable<FixtureResponseDto>>> GenerateFixtures(int id)
    {
        var tournament = await _context.Tournaments
            .Include(t => t.TournamentTeams)
            .ThenInclude(tt => tt.Team)
            .Include(t => t.Fixtures)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tournament == null)
            return NotFound();

        if (tournament.Format != "RoundRobin")
            return BadRequest("Fixture generation currently supports only RoundRobin tournaments.");

        if (tournament.Fixtures.Any())
            return BadRequest("Fixtures have already been generated for this tournament.");

        var teams = tournament.TournamentTeams
            .Select(tt => tt.Team)
            .ToList();

        if (teams.Count < 2)
            return BadRequest("At least 2 teams are required to generate fixtures.");

        var fixtures = new List<Fixture>();
        var currentDate = tournament.StartDate;

        for (int i = 0; i < teams.Count; i++)
        {
            for (int j = i + 1; j < teams.Count; j++)
            {
                if (currentDate > tournament.EndDate)
                    return BadRequest("Tournament date range is not long enough for all fixtures.");

                fixtures.Add(new Fixture
                {
                    TournamentId = tournament.Id,
                    HomeTeamId = teams[i].Id,
                    AwayTeamId = teams[j].Id,
                    MatchDate = currentDate,
                    Status = "Scheduled"
                });

                currentDate = currentDate.AddDays(1);
            }
        }

        tournament.Status = "Scheduled";

        _context.Fixtures.AddRange(fixtures);
        await _context.SaveChangesAsync();

        var response = await _context.Fixtures
            .Where(f => f.TournamentId == tournament.Id)
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

        return response;
    }

[HttpGet("{id}/standings")]
public async Task<ActionResult<IEnumerable<StandingResponseDto>>> GetStandings(int id)
{
    var tournamentExists = await _context.Tournaments.AnyAsync(t => t.Id == id);

    if (!tournamentExists)
        return NotFound("Tournament not found.");

    var standings = await _context.Standings
        .Where(s => s.TournamentId == id)
        .Include(s => s.Team)
        .OrderByDescending(s => s.Points)
        .ThenByDescending(s => s.Wins)
        .Select(s => new StandingResponseDto
        {
            TeamId = s.TeamId,
            TeamName = s.Team.Name,
            Played = s.Played,
            Wins = s.Wins,
            Draws = s.Draws,
            Losses = s.Losses,
            Points = s.Points
        })
        .ToListAsync();

    return standings;
}

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTournament(int id)
    {
        var tournament = await _context.Tournaments.FindAsync(id);

        if (tournament == null)
            return NotFound();

        _context.Tournaments.Remove(tournament);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

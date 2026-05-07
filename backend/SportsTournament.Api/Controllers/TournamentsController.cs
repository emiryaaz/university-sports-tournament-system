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
public async Task<ActionResult<TournamentResponseDto>> CreateTournament(CreateTournamentRequestDto dto)
{
    if (dto.StartDate >= dto.EndDate)
        return BadRequest("Start date must be earlier than end date.");

    if (dto.Format != "RoundRobin" && dto.Format != "SingleElimination")
        return BadRequest("Tournament format must be RoundRobin or SingleElimination.");

    var tournament = new Tournament
    {
        Name = dto.Name,
        SportType = dto.SportType,
        Format = dto.Format,
        Status = "Draft",
        StartDate = dto.StartDate,
        EndDate = dto.EndDate
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
        Teams = new List<string>(),
        Fixtures = new List<FixtureResponseDto>()
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

    if (tournament.Fixtures.Any())
        return BadRequest("Fixtures have already been generated for this tournament.");

    var teams = tournament.TournamentTeams
        .Select(tt => tt.Team)
        .ToList();

    if (teams.Count < 2)
        return BadRequest("At least 2 teams are required to generate fixtures.");

List<Fixture>? fixtures;

try
{
    fixtures = tournament.Format switch
    {
        "RoundRobin" => GenerateRoundRobinFixtures(tournament, teams),
        "SingleElimination" => GenerateSingleEliminationFixtures(tournament, teams),
        _ => null
    };
}
catch (InvalidOperationException ex)
{
    return BadRequest(ex.Message);
}

    if (fixtures == null)
        return BadRequest("Tournament format must be RoundRobin or SingleElimination.");

    tournament.Status = "Scheduled";

    _context.Fixtures.AddRange(fixtures);
    await _context.SaveChangesAsync();

    var response = await _context.Fixtures
        .Where(f => f.TournamentId == tournament.Id)
        .Include(f => f.Tournament)
        .Include(f => f.HomeTeam)
        .Include(f => f.AwayTeam)
        .OrderBy(f => f.RoundNumber)
        .ThenBy(f => f.MatchNumber)
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

private List<Fixture> GenerateRoundRobinFixtures(Tournament tournament, List<Team> teams)
{
    var fixtures = new List<Fixture>();
    var currentDate = tournament.StartDate;
    var matchNumber = 1;

    for (int i = 0; i < teams.Count; i++)
    {
        for (int j = i + 1; j < teams.Count; j++)
        {
            if (currentDate > tournament.EndDate)
                throw new InvalidOperationException("Tournament date range is not long enough for all fixtures.");

            fixtures.Add(new Fixture
            {
                TournamentId = tournament.Id,
                HomeTeamId = teams[i].Id,
                AwayTeamId = teams[j].Id,
                MatchDate = currentDate,
                Status = "Scheduled",
                RoundNumber = 1,
                MatchNumber = matchNumber
            });

            matchNumber++;
            currentDate = currentDate.AddDays(1);
        }
    }

    return fixtures;
}

private List<Fixture> GenerateSingleEliminationFixtures(Tournament tournament, List<Team> teams)
{
    var fixtures = new List<Fixture>();
    var currentDate = tournament.StartDate;

    for (int i = 0; i < teams.Count; i += 2)
    {
        if (i + 1 >= teams.Count)
            break;

        if (currentDate > tournament.EndDate)
            throw new InvalidOperationException("Tournament date range is not long enough for all fixtures.");

        fixtures.Add(new Fixture
        {
            TournamentId = tournament.Id,
            HomeTeamId = teams[i].Id,
            AwayTeamId = teams[i + 1].Id,
            MatchDate = currentDate,
            Status = "Scheduled",
            RoundNumber = 1,
            MatchNumber = (i / 2) + 1
        });

        currentDate = currentDate.AddDays(1);
    }

    return fixtures;
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

[HttpPost("invite-team")]
public async Task<IActionResult> InviteTeamToTournament(InviteTeamToTournamentDto dto)
{
    var tournament = await _context.Tournaments.FindAsync(dto.TournamentId);

    if (tournament == null)
        return NotFound("Tournament not found.");

    var team = await _context.Teams.FindAsync(dto.TeamId);

    if (team == null)
        return NotFound("Team not found.");

    if (team.SportType != tournament.SportType)
        return BadRequest("Team sport type must match tournament sport type.");

    var alreadyRegistered = await _context.TournamentTeams.AnyAsync(tt =>
        tt.TournamentId == dto.TournamentId &&
        tt.TeamId == dto.TeamId);

    if (alreadyRegistered)
        return BadRequest("Team is already registered in this tournament.");

    var existingPendingInvitation = await _context.TournamentInvitations.AnyAsync(i =>
        i.TournamentId == dto.TournamentId &&
        i.TeamId == dto.TeamId &&
        i.Status == "Pending");

    if (existingPendingInvitation)
        return BadRequest("This team already has a pending invitation for this tournament.");

    var invitation = new TournamentInvitation
    {
        TournamentId = dto.TournamentId,
        TeamId = dto.TeamId,
        InvitedByUserId = dto.InvitedByUserId,
        Status = "Pending"
    };

    _context.TournamentInvitations.Add(invitation);
    await _context.SaveChangesAsync();

    return Ok("Tournament invitation sent successfully.");
}

[HttpGet("invitations/team/{teamId}")]
public async Task<IActionResult> GetTournamentInvitationsForTeam(int teamId)
{
    var invitations = await _context.TournamentInvitations
        .Where(i => i.TeamId == teamId && i.Status == "Pending")
        .Include(i => i.Tournament)
        .Include(i => i.InvitedByUser)
        .Select(i => new
        {
            i.Id,
            i.TournamentId,
            TournamentName = i.Tournament.Name,
            SportType = i.Tournament.SportType,
            Format = i.Tournament.Format,
            InvitedBy = i.InvitedByUser.FullName,
            i.Status,
            i.CreatedAt
        })
        .ToListAsync();

    return Ok(invitations);
}

[HttpPost("invitations/respond")]
public async Task<IActionResult> RespondToTournamentInvitation(RespondTournamentInvitationDto dto)
{
    var invitation = await _context.TournamentInvitations
        .Include(i => i.Team)
        .Include(i => i.Tournament)
        .FirstOrDefaultAsync(i => i.Id == dto.InvitationId);

    if (invitation == null)
        return NotFound("Invitation not found.");

    if (invitation.Status != "Pending")
        return BadRequest("Invitation has already been responded.");

    if (invitation.Team.CaptainId != dto.CaptainId)
        return BadRequest("Only the team captain can respond to tournament invitations.");

    if (!dto.Accept)
    {
        invitation.Status = "Rejected";
        await _context.SaveChangesAsync();
        return Ok("Tournament invitation rejected.");
    }

    var alreadyRegistered = await _context.TournamentTeams.AnyAsync(tt =>
        tt.TournamentId == invitation.TournamentId &&
        tt.TeamId == invitation.TeamId);

    if (alreadyRegistered)
        return BadRequest("Team is already registered in this tournament.");

    invitation.Status = "Accepted";

    _context.TournamentTeams.Add(new TournamentTeam
    {
        TournamentId = invitation.TournamentId,
        TeamId = invitation.TeamId
    });

    await _context.SaveChangesAsync();

    return Ok("Tournament invitation accepted. Team registered.");
}

[HttpPost("join-request")]
public async Task<IActionResult> RequestToJoinTournament(RequestToJoinTournamentDto dto)
{
    var tournament = await _context.Tournaments.FindAsync(dto.TournamentId);

    if (tournament == null)
        return NotFound("Tournament not found.");

    var team = await _context.Teams.FindAsync(dto.TeamId);

    if (team == null)
        return NotFound("Team not found.");

    if (team.CaptainId != dto.RequestedByUserId)
        return BadRequest("Only the team captain can request tournament registration.");

    if (team.SportType != tournament.SportType)
        return BadRequest("Team sport type must match tournament sport type.");

    var alreadyRegistered = await _context.TournamentTeams.AnyAsync(tt =>
        tt.TournamentId == dto.TournamentId &&
        tt.TeamId == dto.TeamId);

    if (alreadyRegistered)
        return BadRequest("Team is already registered in this tournament.");

    var existingPendingRequest = await _context.TournamentJoinRequests.AnyAsync(r =>
        r.TournamentId == dto.TournamentId &&
        r.TeamId == dto.TeamId &&
        r.Status == "Pending");

    if (existingPendingRequest)
        return BadRequest("This team already has a pending join request for this tournament.");

    var request = new TournamentJoinRequest
    {
        TournamentId = dto.TournamentId,
        TeamId = dto.TeamId,
        RequestedByUserId = dto.RequestedByUserId,
        Status = "Pending"
    };

    _context.TournamentJoinRequests.Add(request);
    await _context.SaveChangesAsync();

    return Ok("Tournament join request sent successfully.");
}

[HttpGet("join-requests/tournament/{tournamentId}")]
public async Task<IActionResult> GetTournamentJoinRequests(int tournamentId)
{
    var requests = await _context.TournamentJoinRequests
        .Where(r => r.TournamentId == tournamentId && r.Status == "Pending")
        .Include(r => r.Tournament)
        .Include(r => r.Team)
        .Include(r => r.RequestedByUser)
        .Select(r => new
        {
            r.Id,
            r.TournamentId,
            TournamentName = r.Tournament.Name,
            r.TeamId,
            TeamName = r.Team.Name,
            RequestedBy = r.RequestedByUser.FullName,
            r.Status,
            r.CreatedAt
        })
        .ToListAsync();

    return Ok(requests);
}

[HttpPost("join-requests/respond")]
public async Task<IActionResult> RespondToTournamentJoinRequest(RespondTournamentJoinRequestDto dto)
{
    var request = await _context.TournamentJoinRequests
        .Include(r => r.Tournament)
        .Include(r => r.Team)
        .FirstOrDefaultAsync(r => r.Id == dto.JoinRequestId);

    if (request == null)
        return NotFound("Join request not found.");

    if (request.Status != "Pending")
        return BadRequest("Join request has already been responded.");

    var organizer = await _context.Users.FindAsync(dto.OrganizerId);

    if (organizer == null)
        return NotFound("Organizer not found.");

    if (organizer.Role != "TournamentOrganizer" && organizer.Role != "FacilityManager")
        return BadRequest("Only tournament organizers or facility managers can respond to tournament join requests.");

    if (!dto.Accept)
    {
        request.Status = "Rejected";
        await _context.SaveChangesAsync();
        return Ok("Tournament join request rejected.");
    }

    var alreadyRegistered = await _context.TournamentTeams.AnyAsync(tt =>
        tt.TournamentId == request.TournamentId &&
        tt.TeamId == request.TeamId);

    if (alreadyRegistered)
        return BadRequest("Team is already registered in this tournament.");

    request.Status = "Accepted";

    _context.TournamentTeams.Add(new TournamentTeam
    {
        TournamentId = request.TournamentId,
        TeamId = request.TeamId
    });

    await _context.SaveChangesAsync();

    return Ok("Tournament join request accepted. Team registered.");
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

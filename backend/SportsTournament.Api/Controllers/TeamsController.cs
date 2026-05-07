using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsTournament.Api.Data;
using SportsTournament.Api.DTOs;
using SportsTournament.Api.Models;

namespace SportsTournament.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TeamsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeamResponseDto>>> GetTeams()
    {
        var teams = await _context.Teams
            .Include(t => t.Captain)
            .Include(t => t.Members)
            .ThenInclude(m => m.User)
            .Select(t => new TeamResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                SportType = t.SportType,
                CaptainId = t.CaptainId,
                CaptainName = t.Captain.FullName,
                Members = t.Members.Select(m => m.User.FullName).ToList()
            })
            .ToListAsync();

        return teams;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TeamResponseDto>> GetTeam(int id)
    {
        var team = await _context.Teams
            .Include(t => t.Captain)
            .Include(t => t.Members)
            .ThenInclude(m => m.User)
            .Where(t => t.Id == id)
            .Select(t => new TeamResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                SportType = t.SportType,
                CaptainId = t.CaptainId,
                CaptainName = t.Captain.FullName,
                Members = t.Members.Select(m => m.User.FullName).ToList()
            })
            .FirstOrDefaultAsync();

        if (team == null)
            return NotFound();

        return team;
    }

    [HttpPost]
    public async Task<ActionResult<TeamResponseDto>> CreateTeam(CreateTeamRequestDto dto)
    {
        var captain = await _context.Users.FindAsync(dto.CaptainId);

        if (captain == null)
            return BadRequest("Captain user not found.");

        if (captain.Role != "Student" && captain.Role != "FacultyMember")
            return BadRequest("Only students or faculty members can be team captains.");

        var teamNameExists = await _context.Teams.AnyAsync(t => t.Name == dto.Name);

        if (teamNameExists)
            return BadRequest("Team name already exists.");

        var alreadyCaptainInSport = await _context.TeamMembers
            .Include(tm => tm.Team)
            .AnyAsync(tm =>
                tm.UserId == dto.CaptainId &&
                tm.Team.SportType == dto.SportType);

        if (alreadyCaptainInSport)
            return BadRequest("Captain is already in another team for this sport.");

        var team = new Team
        {
            Name = dto.Name,
            SportType = dto.SportType,
            CaptainId = dto.CaptainId,
            Members = new List<TeamMember>
            {
                new TeamMember
                {
                    UserId = dto.CaptainId,
                    MemberRole = "Captain"
                }
            }
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, new TeamResponseDto
        {
            Id = team.Id,
            Name = team.Name,
            SportType = team.SportType,
            CaptainId = team.CaptainId,
            CaptainName = captain.FullName,
            Members = new List<string> { captain.FullName }
        });
    }

    [HttpPost("invite")]
    public async Task<IActionResult> InviteUserToTeam(InviteUserToTeamDto dto)
    {
        var team = await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == dto.TeamId);

        if (team == null)
            return NotFound("Team not found.");

        if (team.CaptainId != dto.InvitedByUserId)
            return BadRequest("Only the team captain can invite users.");

        var invitedUser = await _context.Users.FindAsync(dto.InvitedUserId);

        if (invitedUser == null)
            return NotFound("Invited user not found.");

        if (team.Members.Any(m => m.UserId == dto.InvitedUserId))
            return BadRequest("User is already a member of this team.");

        var alreadyInSportTeam = await _context.TeamMembers
            .Include(tm => tm.Team)
            .AnyAsync(tm =>
                tm.UserId == dto.InvitedUserId &&
                tm.Team.SportType == team.SportType);

        if (alreadyInSportTeam)
            return BadRequest("User is already in another team for this sport.");

        var existingPendingInvitation = await _context.TeamInvitations.AnyAsync(i =>
            i.TeamId == dto.TeamId &&
            i.InvitedUserId == dto.InvitedUserId &&
            i.Status == "Pending");

        if (existingPendingInvitation)
            return BadRequest("This user already has a pending invitation for this team.");

        var invitation = new TeamInvitation
        {
            TeamId = dto.TeamId,
            InvitedUserId = dto.InvitedUserId,
            InvitedByUserId = dto.InvitedByUserId,
            Status = "Pending"
        };

        _context.TeamInvitations.Add(invitation);
        await _context.SaveChangesAsync();

        return Ok("Invitation sent successfully.");
    }

    [HttpGet("invitations/user/{userId}")]
    public async Task<IActionResult> GetUserInvitations(int userId)
    {
        var invitations = await _context.TeamInvitations
            .Where(i => i.InvitedUserId == userId && i.Status == "Pending")
            .Include(i => i.Team)
            .Include(i => i.InvitedByUser)
            .Select(i => new
            {
                i.Id,
                i.TeamId,
                TeamName = i.Team.Name,
                SportType = i.Team.SportType,
                InvitedBy = i.InvitedByUser.FullName,
                i.Status,
                i.CreatedAt
            })
            .ToListAsync();

        return Ok(invitations);
    }

    [HttpPost("invitations/respond")]
    public async Task<IActionResult> RespondToInvitation(RespondTeamInvitationDto dto)
    {
        var invitation = await _context.TeamInvitations
            .Include(i => i.Team)
            .FirstOrDefaultAsync(i => i.Id == dto.InvitationId);

        if (invitation == null)
            return NotFound("Invitation not found.");

        if (invitation.Status != "Pending")
            return BadRequest("Invitation has already been responded.");

        if (!dto.Accept)
        {
            invitation.Status = "Rejected";
            await _context.SaveChangesAsync();
            return Ok("Invitation rejected.");
        }

        var alreadyInSportTeam = await _context.TeamMembers
            .Include(tm => tm.Team)
            .AnyAsync(tm =>
                tm.UserId == invitation.InvitedUserId &&
                tm.Team.SportType == invitation.Team.SportType);

        if (alreadyInSportTeam)
            return BadRequest("User is already in another team for this sport.");

        invitation.Status = "Accepted";

        _context.TeamMembers.Add(new TeamMember
        {
            TeamId = invitation.TeamId,
            UserId = invitation.InvitedUserId,
            MemberRole = "Player"
        });

        await _context.SaveChangesAsync();

        return Ok("Invitation accepted. User joined the team.");
    }

    [HttpPost("join-request")]
    public async Task<IActionResult> RequestToJoinTeam(RequestToJoinTeamDto dto)
    {
        var team = await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == dto.TeamId);

        if (team == null)
            return NotFound("Team not found.");

        var requestedUser = await _context.Users.FindAsync(dto.RequestedUserId);

        if (requestedUser == null)
            return NotFound("User not found.");

        if (team.Members.Any(m => m.UserId == dto.RequestedUserId))
            return BadRequest("User is already a member of this team.");

        var alreadyInSportTeam = await _context.TeamMembers
            .Include(tm => tm.Team)
            .AnyAsync(tm =>
                tm.UserId == dto.RequestedUserId &&
                tm.Team.SportType == team.SportType);

        if (alreadyInSportTeam)
            return BadRequest("User is already in another team for this sport.");

        var existingPendingRequest = await _context.TeamJoinRequests.AnyAsync(r =>
            r.TeamId == dto.TeamId &&
            r.RequestedUserId == dto.RequestedUserId &&
            r.Status == "Pending");

        if (existingPendingRequest)
            return BadRequest("This user already has a pending join request for this team.");

        var request = new TeamJoinRequest
        {
            TeamId = dto.TeamId,
            RequestedUserId = dto.RequestedUserId,
            Status = "Pending"
        };

        _context.TeamJoinRequests.Add(request);
        await _context.SaveChangesAsync();

        return Ok("Join request sent successfully.");
    }

    [HttpGet("join-requests/team/{teamId}")]
    public async Task<IActionResult> GetTeamJoinRequests(int teamId)
    {
        var requests = await _context.TeamJoinRequests
            .Where(r => r.TeamId == teamId && r.Status == "Pending")
            .Include(r => r.RequestedUser)
            .Include(r => r.Team)
            .Select(r => new
            {
                r.Id,
                r.TeamId,
                TeamName = r.Team.Name,
                RequestedUserId = r.RequestedUserId,
                RequestedUser = r.RequestedUser.FullName,
                r.Status,
                r.CreatedAt
            })
            .ToListAsync();

        return Ok(requests);
    }

    [HttpPost("join-requests/respond")]
    public async Task<IActionResult> RespondToJoinRequest(RespondJoinRequestDto dto)
    {
        var request = await _context.TeamJoinRequests
            .Include(r => r.Team)
            .FirstOrDefaultAsync(r => r.Id == dto.JoinRequestId);

        if (request == null)
            return NotFound("Join request not found.");

        if (request.Status != "Pending")
            return BadRequest("Join request has already been responded.");

        if (request.Team.CaptainId != dto.CaptainId)
            return BadRequest("Only the team captain can respond to join requests.");

        if (!dto.Accept)
        {
            request.Status = "Rejected";
            await _context.SaveChangesAsync();
            return Ok("Join request rejected.");
        }

        var alreadyInSportTeam = await _context.TeamMembers
            .Include(tm => tm.Team)
            .AnyAsync(tm =>
                tm.UserId == request.RequestedUserId &&
                tm.Team.SportType == request.Team.SportType);

        if (alreadyInSportTeam)
            return BadRequest("User is already in another team for this sport.");

        request.Status = "Accepted";

        _context.TeamMembers.Add(new TeamMember
        {
            TeamId = request.TeamId,
            UserId = request.RequestedUserId,
            MemberRole = "Player"
        });

        await _context.SaveChangesAsync();

        return Ok("Join request accepted. User joined the team.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTeam(int id)
    {
        var team = await _context.Teams.FindAsync(id);

        if (team == null)
            return NotFound();

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

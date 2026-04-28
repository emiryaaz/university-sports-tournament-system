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
    public async Task<ActionResult<TeamResponseDto>> CreateTeam(CreateTeamDto dto)
    {
        var captain = await _context.Users.FindAsync(dto.CaptainId);

        if (captain == null)
            return BadRequest("Captain user not found.");

        if (captain.Role != "Student" && captain.Role != "FacultyMember")
            return BadRequest("Only students or faculty members can be team captains.");

        var teamNameExists = await _context.Teams.AnyAsync(t => t.Name == dto.Name);

        if (teamNameExists)
            return BadRequest("Team name already exists.");

        var memberIds = dto.MemberUserIds.Distinct().ToList();

        if (!memberIds.Contains(dto.CaptainId))
            memberIds.Add(dto.CaptainId);

        var users = await _context.Users
            .Where(u => memberIds.Contains(u.Id))
            .ToListAsync();

        if (users.Count != memberIds.Count)
            return BadRequest("One or more users were not found.");

        var invalidUsers = users
            .Where(u => u.Role != "Student" && u.Role != "FacultyMember")
            .ToList();

        if (invalidUsers.Any())
            return BadRequest("All team members must be students or faculty members.");

        var alreadyInTeamForSport = await _context.TeamMembers
            .Include(tm => tm.Team)
            .AnyAsync(tm =>
                memberIds.Contains(tm.UserId) &&
                tm.Team.SportType == dto.SportType);

        if (alreadyInTeamForSport)
            return BadRequest("One or more users are already registered in another team for this sport.");

        var team = new Team
        {
            Name = dto.Name,
            SportType = dto.SportType,
            CaptainId = dto.CaptainId,
            Members = memberIds.Select(userId => new TeamMember
            {
                UserId = userId,
                MemberRole = userId == dto.CaptainId ? "Captain" : "Player"
            }).ToList()
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        var response = new TeamResponseDto
        {
            Id = team.Id,
            Name = team.Name,
            SportType = team.SportType,
            CaptainId = team.CaptainId,
            CaptainName = captain.FullName,
            Members = users.Select(u => u.FullName).ToList()
        };

        return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, response);
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

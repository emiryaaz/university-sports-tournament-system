using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsTournament.Api.Data;

namespace SportsTournament.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public DevController(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpDelete("reset-database")]
    public async Task<IActionResult> ResetDatabase()
    {
        if (!_environment.IsDevelopment())
            return Forbid("Database reset is only allowed in Development environment.");

        await _context.MatchResults.ExecuteDeleteAsync();
        await _context.Standings.ExecuteDeleteAsync();
        await _context.Fixtures.ExecuteDeleteAsync();
        await _context.TournamentTeams.ExecuteDeleteAsync();
        await _context.Tournaments.ExecuteDeleteAsync();
        await _context.TeamMembers.ExecuteDeleteAsync();
        await _context.Teams.ExecuteDeleteAsync();
        await _context.Facilities.ExecuteDeleteAsync();
        await _context.Users.ExecuteDeleteAsync();

        await _context.Database.ExecuteSqlRawAsync(@"
            ALTER SEQUENCE ""Users_Id_seq"" RESTART WITH 1;
            ALTER SEQUENCE ""Teams_Id_seq"" RESTART WITH 1;
            ALTER SEQUENCE ""TeamMembers_Id_seq"" RESTART WITH 1;
            ALTER SEQUENCE ""Tournaments_Id_seq"" RESTART WITH 1;
            ALTER SEQUENCE ""TournamentTeams_Id_seq"" RESTART WITH 1;
            ALTER SEQUENCE ""Fixtures_Id_seq"" RESTART WITH 1;
            ALTER SEQUENCE ""MatchResults_Id_seq"" RESTART WITH 1;
            ALTER SEQUENCE ""Standings_Id_seq"" RESTART WITH 1;
            ALTER SEQUENCE ""Facilities_Id_seq"" RESTART WITH 1;
        ");

        return Ok("Database reset successfully. All data and IDs were cleared.");
    }
}

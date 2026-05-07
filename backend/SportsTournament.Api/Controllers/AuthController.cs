using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsTournament.Api.Data;
using SportsTournament.Api.DTOs;
using SportsTournament.Api.Models;

namespace SportsTournament.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponseDto>> Register(RegisterDto dto)
    {
        var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);

        if (emailExists)
            return BadRequest("Email already exists.");

        if (dto.Password.Length < 6)
            return BadRequest("Password must be at least 6 characters.");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Teams = new List<string>()
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserResponseDto>> Login(LoginDto dto)
    {
        var user = await _context.Users
            .Include(u => u.TeamMemberships)
            .ThenInclude(tm => tm.Team)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
            return BadRequest("Invalid email or password.");

        var passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

        if (!passwordValid)
            return BadRequest("Invalid email or password.");

        return new UserResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Teams = user.TeamMemberships.Select(tm => tm.Team.Name).ToList()
        };
    }
}

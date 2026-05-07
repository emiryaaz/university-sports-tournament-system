namespace SportsTournament.Api.Models;

public class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty; 

    public string Role { get; set; } = "Student";

    public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();
}

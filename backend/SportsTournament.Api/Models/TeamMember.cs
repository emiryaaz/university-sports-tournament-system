namespace SportsTournament.Api.Models;

public class TeamMember
{
    public int Id { get; set; }

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string MemberRole { get; set; } = "Player";

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

namespace SportsTournament.Api.Models;

public class TeamInvitation
{
    public int Id { get; set; }

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public int InvitedUserId { get; set; }
    public User InvitedUser { get; set; } = null!;

    public int InvitedByUserId { get; set; }
    public User InvitedByUser { get; set; } = null!;

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

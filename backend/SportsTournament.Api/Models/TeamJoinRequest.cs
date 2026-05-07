namespace SportsTournament.Api.Models;

public class TeamJoinRequest
{
    public int Id { get; set; }

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public int RequestedUserId { get; set; }
    public User RequestedUser { get; set; } = null!;

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

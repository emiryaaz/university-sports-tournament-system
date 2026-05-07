namespace SportsTournament.Api.Models;

public class TournamentInvitation
{
    public int Id { get; set; }

    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public int InvitedByUserId { get; set; }
    public User InvitedByUser { get; set; } = null!;

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

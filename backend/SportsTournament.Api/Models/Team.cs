namespace SportsTournament.Api.Models;

public class Team
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SportType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CaptainId { get; set; }
    public User Captain { get; set; } = null!;

    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();

    public ICollection<TournamentTeam> TournamentTeams { get; set; } = new List<TournamentTeam>();
}

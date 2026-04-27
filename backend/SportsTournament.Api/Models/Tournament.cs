namespace SportsTournament.Api.Models;

public class Tournament
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SportType { get; set; } = string.Empty;

    public string Format { get; set; } = "RoundRobin";

    public string Status { get; set; } = "Draft";

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public ICollection<TournamentTeam> TournamentTeams { get; set; } = new List<TournamentTeam>();

    public ICollection<Fixture> Fixtures { get; set; } = new List<Fixture>();

    public ICollection<Standing> Standings { get; set; } = new List<Standing>();
}

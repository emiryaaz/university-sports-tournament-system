namespace SportsTournament.Api.Models;

public class Fixture
{
    public int Id { get; set; }

    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;

    public int HomeTeamId { get; set; }
    public Team HomeTeam { get; set; } = null!;

    public int AwayTeamId { get; set; }
    public Team AwayTeam { get; set; } = null!;

    public int? FacilityId { get; set; }
    public Facility? Facility { get; set; }

    public DateTime MatchDate { get; set; }

    public string Status { get; set; } = "Scheduled";

public int RoundNumber { get; set; } = 1;

public int MatchNumber { get; set; } = 1;

    public MatchResult? MatchResult { get; set; }
}

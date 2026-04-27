namespace SportsTournament.Api.Models;

public class MatchResult
{
    public int Id { get; set; }

    public int FixtureId { get; set; }
    public Fixture Fixture { get; set; } = null!;

    public int HomeScore { get; set; }

    public int AwayScore { get; set; }

    public int WinnerTeamId { get; set; }
    public Team WinnerTeam { get; set; } = null!;
}

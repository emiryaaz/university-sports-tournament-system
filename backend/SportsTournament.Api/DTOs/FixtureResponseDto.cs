namespace SportsTournament.Api.DTOs;

public class FixtureResponseDto
{
    public int Id { get; set; }

    public int TournamentId { get; set; }

    public string TournamentName { get; set; } = string.Empty;

    public int HomeTeamId { get; set; }

    public string HomeTeamName { get; set; } = string.Empty;

    public int AwayTeamId { get; set; }

    public string AwayTeamName { get; set; } = string.Empty;

    public DateTime MatchDate { get; set; }

    public string Status { get; set; } = string.Empty;
}

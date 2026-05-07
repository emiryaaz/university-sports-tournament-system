namespace SportsTournament.Api.DTOs;

public class MatchResultResponseDto
{
    public int FixtureId { get; set; }

    public string TournamentName { get; set; } = string.Empty;

    public string HomeTeamName { get; set; } = string.Empty;

    public int HomeScore { get; set; }

    public string AwayTeamName { get; set; } = string.Empty;

    public int AwayScore { get; set; }

    public string? WinnerTeamName { get; set; }
}

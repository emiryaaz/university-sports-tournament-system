namespace SportsTournament.Api.DTOs;

public class StandingResponseDto
{
    public int TeamId { get; set; }

    public string TeamName { get; set; } = string.Empty;

    public int Played { get; set; }

    public int Wins { get; set; }

    public int Draws { get; set; }

    public int Losses { get; set; }

    public int Points { get; set; }
}

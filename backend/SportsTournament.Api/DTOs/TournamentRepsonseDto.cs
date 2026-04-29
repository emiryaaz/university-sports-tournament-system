namespace SportsTournament.Api.DTOs;

public class TournamentResponseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SportType { get; set; } = string.Empty;

    public string Format { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public List<string> Teams { get; set; } = new();

    public List<FixtureResponseDto> Fixtures { get; set; } = new();
}

namespace SportsTournament.Api.DTOs;

public class CreateTournamentDto
{
    public string Name { get; set; } = string.Empty;

    public string SportType { get; set; } = string.Empty;

    public string Format { get; set; } = "RoundRobin";

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public List<int> TeamIds { get; set; } = new();
}

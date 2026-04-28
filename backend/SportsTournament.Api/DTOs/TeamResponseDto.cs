namespace SportsTournament.Api.DTOs;

public class TeamResponseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SportType { get; set; } = string.Empty;

    public int CaptainId { get; set; }

    public string CaptainName { get; set; } = string.Empty;

    public List<string> Members { get; set; } = new();
}

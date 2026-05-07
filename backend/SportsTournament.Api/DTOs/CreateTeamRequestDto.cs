namespace SportsTournament.Api.DTOs;

public class CreateTeamRequestDto
{
    public string Name { get; set; } = string.Empty;

    public string SportType { get; set; } = string.Empty;

    public int CaptainId { get; set; }
}

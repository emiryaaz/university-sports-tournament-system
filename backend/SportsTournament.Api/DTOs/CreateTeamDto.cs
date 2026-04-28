namespace SportsTournament.Api.DTOs;

public class CreateTeamDto
{
    public string Name { get; set; } = string.Empty;

    public string SportType { get; set; } = string.Empty;

    public int CaptainId { get; set; }

    public List<int> MemberUserIds { get; set; } = new();
}

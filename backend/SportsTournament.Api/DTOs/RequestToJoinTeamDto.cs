namespace SportsTournament.Api.DTOs;

public class RequestToJoinTeamDto
{
    public int TeamId { get; set; }

    public int RequestedUserId { get; set; }
}

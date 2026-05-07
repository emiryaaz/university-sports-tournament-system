namespace SportsTournament.Api.DTOs;

public class InviteUserToTeamDto
{
    public int TeamId { get; set; }

    public int InvitedUserId { get; set; }

    public int InvitedByUserId { get; set; }
}

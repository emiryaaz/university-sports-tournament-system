namespace SportsTournament.Api.DTOs;

public class RespondTournamentInvitationDto
{
    public int InvitationId { get; set; }

    public bool Accept { get; set; }

    public int CaptainId { get; set; }
}

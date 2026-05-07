namespace SportsTournament.Api.DTOs;

public class RespondTournamentJoinRequestDto
{
    public int JoinRequestId { get; set; }

    public bool Accept { get; set; }

    public int OrganizerId { get; set; }
}

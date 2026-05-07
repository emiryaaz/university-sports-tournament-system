namespace SportsTournament.Api.DTOs;

public class RequestToJoinTournamentDto
{
    public int TournamentId { get; set; }

    public int TeamId { get; set; }

    public int RequestedByUserId { get; set; }
}

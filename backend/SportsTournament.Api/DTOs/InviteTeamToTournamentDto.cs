namespace SportsTournament.Api.DTOs;

public class InviteTeamToTournamentDto
{
    public int TournamentId { get; set; }

    public int TeamId { get; set; }

    public int InvitedByUserId { get; set; }
}

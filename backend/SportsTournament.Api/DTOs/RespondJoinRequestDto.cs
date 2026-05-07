namespace SportsTournament.Api.DTOs;

public class RespondJoinRequestDto
{
    public int JoinRequestId { get; set; }

    public bool Accept { get; set; }

    public int CaptainId { get; set; }
}

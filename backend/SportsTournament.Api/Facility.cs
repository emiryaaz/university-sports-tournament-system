namespace SportsTournament.Api.Models;

public class Facility
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SportType { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public string Status { get; set; } = "Available";

    public ICollection<Fixture> Fixtures { get; set; } = new List<Fixture>();
}

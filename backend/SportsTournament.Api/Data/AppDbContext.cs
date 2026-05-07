using Microsoft.EntityFrameworkCore;
using SportsTournament.Api.Models;

namespace SportsTournament.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<TeamInvitation> TeamInvitations => Set<TeamInvitation>();
    public DbSet<TeamJoinRequest> TeamJoinRequests => Set<TeamJoinRequest>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<TournamentTeam> TournamentTeams => Set<TournamentTeam>();
    public DbSet<TournamentInvitation> TournamentInvitations => Set<TournamentInvitation>();
    public DbSet<TournamentJoinRequest> TournamentJoinRequests => Set<TournamentJoinRequest>();
    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<Fixture> Fixtures => Set<Fixture>();
    public DbSet<MatchResult> MatchResults => Set<MatchResult>();
    public DbSet<Standing> Standings => Set<Standing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Team>()
            .HasIndex(t => t.Name)
            .IsUnique();

        modelBuilder.Entity<TeamMember>()
            .HasIndex(tm => new { tm.TeamId, tm.UserId })
            .IsUnique();

        modelBuilder.Entity<TournamentTeam>()
            .HasIndex(tt => new { tt.TournamentId, tt.TeamId })
            .IsUnique();

	modelBuilder.Entity<TournamentInvitation>()
            .HasIndex(i => new { i.TournamentId, i.TeamId, i.Status });

	modelBuilder.Entity<TournamentJoinRequest>()
            .HasIndex(r => new { r.TournamentId, r.TeamId, r.Status });

        modelBuilder.Entity<Fixture>()
            .HasOne(f => f.HomeTeam)
            .WithMany()
            .HasForeignKey(f => f.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Fixture>()
            .HasOne(f => f.AwayTeam)
            .WithMany()
            .HasForeignKey(f => f.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MatchResult>()
            .HasOne(mr => mr.Fixture)
            .WithOne(f => f.MatchResult)
            .HasForeignKey<MatchResult>(mr => mr.FixtureId);

        modelBuilder.Entity<Standing>()
            .HasIndex(s => new { s.TournamentId, s.TeamId })
            .IsUnique();
    }
}

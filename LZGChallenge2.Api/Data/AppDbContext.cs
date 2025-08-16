using Microsoft.EntityFrameworkCore;
using LZGChallenge2.Api.Models;

namespace LZGChallenge2.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<Player> Players { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<PlayerStats> PlayerStats { get; set; }
    public DbSet<ChampionStats> ChampionStats { get; set; }
    public DbSet<RoleStats> RoleStats { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuration Player
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.Puuid).IsUnique();
            entity.HasIndex(p => new { p.GameName, p.TagLine }).IsUnique();
            entity.HasIndex(p => p.SummonerId).IsUnique();
            
            entity.Property(p => p.RiotId).IsRequired().HasMaxLength(50);
            entity.Property(p => p.GameName).IsRequired().HasMaxLength(50);
            entity.Property(p => p.TagLine).IsRequired().HasMaxLength(10);
            entity.Property(p => p.Puuid).IsRequired().HasMaxLength(78);
            entity.Property(p => p.SummonerId).IsRequired().HasMaxLength(78);
            entity.Property(p => p.Region).IsRequired().HasMaxLength(10);
        });
        
        // Configuration Match
        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.HasIndex(m => m.MatchId);
            entity.HasIndex(m => new { m.PlayerId, m.MatchId }).IsUnique();
            entity.HasIndex(m => m.GameStartTime);
            
            entity.Property(m => m.MatchId).IsRequired().HasMaxLength(15);
            entity.Property(m => m.ChampionName).IsRequired().HasMaxLength(50);
            entity.Property(m => m.Position).IsRequired().HasMaxLength(20);
            
            entity.HasOne(m => m.Player)
                .WithMany(p => p.Matches)
                .HasForeignKey(m => m.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configuration PlayerStats
        modelBuilder.Entity<PlayerStats>(entity =>
        {
            entity.HasKey(ps => ps.Id);
            entity.HasIndex(ps => ps.PlayerId).IsUnique();
            
            entity.Property(ps => ps.CurrentTier).HasMaxLength(20);
            entity.Property(ps => ps.CurrentRank).HasMaxLength(5);
            
            entity.HasOne(ps => ps.Player)
                .WithOne(p => p.CurrentStats)
                .HasForeignKey<PlayerStats>(ps => ps.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configuration ChampionStats
        modelBuilder.Entity<ChampionStats>(entity =>
        {
            entity.HasKey(cs => cs.Id);
            entity.HasIndex(cs => new { cs.PlayerId, cs.ChampionId }).IsUnique();
            
            entity.Property(cs => cs.ChampionName).IsRequired().HasMaxLength(50);
            
            entity.HasOne(cs => cs.Player)
                .WithMany(p => p.ChampionStats)
                .HasForeignKey(cs => cs.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configuration RoleStats
        modelBuilder.Entity<RoleStats>(entity =>
        {
            entity.HasKey(rs => rs.Id);
            entity.HasIndex(rs => new { rs.PlayerId, rs.Position }).IsUnique();
            
            entity.Property(rs => rs.Position).IsRequired().HasMaxLength(20);
            
            entity.HasOne(rs => rs.Player)
                .WithMany(p => p.RoleStats)
                .HasForeignKey(rs => rs.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configuration des types décimaux pour éviter les problèmes de précision
        modelBuilder.Entity<PlayerStats>()
            .Property(ps => ps.TotalKills)
            .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<PlayerStats>()
            .Property(ps => ps.TotalDeaths)
            .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<PlayerStats>()
            .Property(ps => ps.TotalAssists)
            .HasColumnType("decimal(18,2)");
        
        modelBuilder.Entity<ChampionStats>()
            .Property(cs => cs.TotalKills)
            .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<ChampionStats>()
            .Property(cs => cs.TotalDeaths)
            .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<ChampionStats>()
            .Property(cs => cs.TotalAssists)
            .HasColumnType("decimal(18,2)");
        
        modelBuilder.Entity<RoleStats>()
            .Property(rs => rs.TotalKills)
            .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<RoleStats>()
            .Property(rs => rs.TotalDeaths)
            .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<RoleStats>()
            .Property(rs => rs.TotalAssists)
            .HasColumnType("decimal(18,2)");
    }
}
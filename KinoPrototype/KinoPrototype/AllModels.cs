using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace KinoPrototype;

public class Host
{
    [Key] public string AuthId { get; set; }
    public string Username { get; set; }
    public List<JoinEvent> JoinEvents { get; set; }
    
}

public class Participant
{
    public int Id { get; set; }
    public string Nickname { get; set; }
    public List<Showtime>? VotedFor { get; set; }
}

public class JoinEvent
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    
    public List<Showtime>? Showtimes { get; set; }
    public List<Participant> Participants { get; set; }
    private DateTime _deadline;

    public DateTime Deadline
    {
        get => _deadline;
        set => _deadline = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }
    
    // nagivation property to Host
    public Host Host { get; set; }
}

public class Movie
{
    public int Id { get; set; }
    public string Navn { get; set; }
    public List<Showtime>? Showtimes { get; set; }
    public string ImageUrl { get; set; }
    public int Duration { get; set; }
    private DateTime _premiereDate;

    public DateTime PremiereDate
    {
        get => _premiereDate;
        set => _premiereDate = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }

    public int AgeRating { get; set; }
}

public class Showtime
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public Movie Movie { get; set; }
    public int CinemaId { get; set; }
    public Cinema Cinema { get; set; }
    public int PlaytimeId { get; set; }
    public Playtime Playtime { get; set; }
    public int VersionTagId { get; set; }
    public VersionTag VersionTag { get; set; }
    public int SalId { get; set; }
    public Sal Sal { get; set; }

    //Many to many to JoinEvent
    public List<JoinEvent> JoinEvents { get; set; }

    //Many to many to Participant
    public List<Participant> Participants { get; set; }
}

public class Playtime
{
    public int Id { get; set; }
    private DateTime _startTime;

    public DateTime StartTime
    {
        get => _startTime;
        set => _startTime = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }
}

public class VersionTag
{
    public int Id { get; set; }
    public string Type { get; set; }
}

public class Cinema
{
    public int Id { get; set; }
    public string Navn { get; set; }
}

public class Sal
{
    public int Id { get; set; }
    public string Navn { get; set; }
}

public class KinoContext : DbContext
{
    //For KinoJoin
    public DbSet<Host> Hosts { get; set; }
    public DbSet<Participant> Participants { get; set; }
    public DbSet<JoinEvent> JoinEvents { get; set; }

    //For basedata from Kino
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Showtime> Showtimes { get; set; }
    public DbSet<Playtime> Playtimes { get; set; }
    public DbSet<VersionTag> Versions { get; set; }
    public DbSet<Cinema> Cinemas { get; set; }
    public DbSet<Sal> Sals { get; set; }

    public KinoContext(DbContextOptions<KinoContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Host>()
            .HasKey(h => h.AuthId);

        modelBuilder.Entity<Participant>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<JoinEvent>()
            .HasKey(je => je.Id);

        modelBuilder.Entity<JoinEvent>()
            .HasMany(je => je.Showtimes)
            .WithMany(s => s.JoinEvents);

        modelBuilder.Entity<Participant>().HasMany(s => s.VotedFor).WithMany(je => je.Participants);
        
        // Configure primary keys
        modelBuilder.Entity<Movie>().HasKey(m => m.Id);
        modelBuilder.Entity<Showtime>().HasKey(s => s.Id);
        modelBuilder.Entity<Playtime>().HasKey(p => p.Id);
        modelBuilder.Entity<VersionTag>().HasKey(v => v.Id);
        modelBuilder.Entity<Cinema>().HasKey(c => c.Id);
        modelBuilder.Entity<Sal>().HasKey(s => s.Id);

        // Configure relationships for Movie and Showtime
        modelBuilder.Entity<Movie>()
            .HasMany(m => m.Showtimes)
            .WithOne(s => s.Movie)
            .HasForeignKey(s => s.MovieId);

        // Make showtime key MovieId, CinemaId, ShowtimeId, VersionId, SalId
        modelBuilder.Entity<Showtime>()
            .HasKey(s => new { s.MovieId, s.CinemaId, ShowtimeId = s.PlaytimeId, VersionId = s.VersionTagId, s.SalId });

        // Call the base method to ensure any configuration from the base class is applied
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }
}
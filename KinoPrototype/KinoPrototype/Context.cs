using KinoPrototype.Domain;
using Microsoft.EntityFrameworkCore;

namespace KinoPrototype;

public class KinoContext : DbContext
{
    //For KinoJoin
    public DbSet<Domain.Host> Hosts { get; set; }
    public DbSet<Participant> Participants { get; set; }
    public DbSet<JoinEvent> JoinEvents { get; set; }

    //For basedata from Kino
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Showtime> Showtimes { get; set; }
    public DbSet<Playtime> Playtimes { get; set; }
    public DbSet<VersionTag> Versions { get; set; }
    public DbSet<Cinema> Cinemas { get; set; }
    public DbSet<Room> Sals { get; set; }

    public KinoContext(DbContextOptions<KinoContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Host>()
            .HasKey(h => h.AuthId);

        modelBuilder.Entity<JoinEvent>()
            .HasKey(je => je.Id);

        modelBuilder.Entity<JoinEvent>()
            .HasMany(je => je.Showtimes)
            .WithMany(s => s.JoinEvents);
        
        modelBuilder.Entity<JoinEvent>().HasMany(je=>je.Participants).WithOne(p=>p.JoinEvent);

        modelBuilder.Entity<Participant>().HasMany(s => s.VotedFor).WithMany(je => je.Participants);
        
        // Configure primary keys
        modelBuilder.Entity<Movie>().HasKey(m => m.Id);
        modelBuilder.Entity<Showtime>().HasKey(s => s.Id);
        modelBuilder.Entity<Playtime>().HasKey(p => p.Id);
        modelBuilder.Entity<VersionTag>().HasKey(v => v.Id);
        modelBuilder.Entity<Cinema>().HasKey(c => c.Id);
        modelBuilder.Entity<Room>().HasKey(s => s.Id);

        // Configure relationships for Movie and Showtime
        modelBuilder.Entity<Movie>()
            .HasMany(m => m.Showtimes)
            .WithOne(s => s.Movie)
            .HasForeignKey(s => s.MovieId);

        // Make showtime key MovieId, CinemaId, ShowtimeId, VersionId, SalId
        modelBuilder.Entity<Showtime>()
            .HasKey(st => st.Id);

        // Call the base method to ensure any configuration from the base class is applied
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }
}
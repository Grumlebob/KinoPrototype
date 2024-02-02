using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KinoPrototype.Domain;

public class Host
{
    [Key] public string AuthId { get; set; }
    public string Username { get; set; }
    public List<JoinEvent>? JoinEvents { get; set; }
}

public class Participant
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Nickname { get; set; }
    
    public List<Showtime>? VotedFor { get; set; }
}

public class JoinEvent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string? HostId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public List<Showtime>? Showtimes { get; set; }

    public List<Participant>? Participants { get; set; }
    private DateTime _deadline;

    public DateTime Deadline
    {
        get => _deadline;
        set => _deadline = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }

    [ForeignKey("HostId")] public Host? Host { get; set; }
}

public class Movie
{
    [Key] public int Id { get; set; }
    public string Navn { get; set; }
    public List<Showtime>? Showtimes { get; set; }
    public string ImageUrl { get; set; }
    public int Duration { get; set; }

    public string PremiereDate { get; set; }

    public string AgeRating { get; set; }
}

public class Showtime
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int MovieId { get; set; }
    public int CinemaId { get; set; }
    public int PlaytimeId { get; set; }
    public int VersionTagId { get; set; }
    public int SalId { get; set; }

    //Many to many to JoinEvent
    public List<JoinEvent> JoinEvents { get; set; }

    //Many to many to Participant
    public List<Participant> Participants { get; set; }

    //Foreign Keys
    [ForeignKey("VersionTagId")] public VersionTag VersionTag { get; set; }
    [ForeignKey("SalId")] public Sal Sal { get; set; }
    [ForeignKey("MovieId")] public Movie Movie { get; set; }
    [ForeignKey("CinemaId")] public Cinema Cinema { get; set; }
    [ForeignKey("PlaytimeId")] public Playtime Playtime { get; set; }
}

public class Playtime
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Type { get; set; }
}

public class Cinema
{
    [Key] public int Id { get; set; }
    public string Navn { get; set; }
}

public class Sal
{
    [Key]
    public int Id { get; set; }

    public string Navn { get; set; }
}
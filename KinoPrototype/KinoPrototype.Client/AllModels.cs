using System.ComponentModel.DataAnnotations;

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

    public  DateTime PremiereDate { get; set; }
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
    public int VersionId { get; set; }
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


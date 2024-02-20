using KinoPrototype.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KinoPrototype;

public static class AllEndpoints
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        app.MapGet("/hello", () => "Hello, World!");
        app.MapGet("/json", () => JsonParser.getJsonString());
        app.MapGet("events/{hostId}", async (string hostId) =>
        {
            using var scope = app.Services.CreateScope();

            await using var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<KinoContext>();

            var results = await context.JoinEvents.Where(j => j.HostId == hostId).Select(
                    e => new JoinEvent
                    {
                        Id = e.Id, Title = e.Title, Description = e.Description, Deadline = e.Deadline, Host = e.Host,
                        Showtimes = e.Showtimes.Select(s => new Showtime
                        {
                            Id = s.Id,
                            Movie = new Movie()
                            {
                                Id = s.MovieId, AgeRating = s.Movie.AgeRating, PremiereDate = s.Movie.PremiereDate,
                                Title = s.Movie.Title, ImageUrl = s.Movie.ImageUrl, Duration = s.Movie.Duration
                            },
                            Cinema = s.Cinema, Playtime = s.Playtime, Room = s.Room,
                            VersionTag = s.VersionTag,
                        }).ToList(),
                        Participants = e.Participants
                    })
                .ToListAsync();


            //create empty participant lists if they are null
            foreach (var joinEvent in results)
            {
                joinEvent.Participants ??= new List<Participant>();
            }

            return Results.Ok(results);
        });
        app.MapGet("/event/{id}", async (int id) =>
        {
            using var scope = app.Services.CreateScope();

            var result = await scope.ServiceProvider.GetRequiredService<KinoContext>().JoinEvents.Select(
                    e => new JoinEvent
                    {
                        Id = e.Id, Title = e.Title, Description = e.Description, Deadline = e.Deadline, Host = e.Host,
                        Participants = e.Participants.Select(p => new Participant
                        {
                            Id = p.Id, Nickname = p.Nickname, VotedFor = p.VotedFor
                        }).ToList(),
                        Showtimes = e.Showtimes.Select(s => new Showtime
                        {
                            Id = s.Id,
                            Movie = new Movie()
                            {
                                Id = s.MovieId, AgeRating = s.Movie.AgeRating, PremiereDate = s.Movie.PremiereDate,
                                Title = s.Movie.Title, ImageUrl = s.Movie.ImageUrl, Duration = s.Movie.Duration,
                            },
                            Cinema = s.Cinema, Playtime = s.Playtime, Room = s.Room,
                            VersionTag = s.VersionTag,
                        }).ToList()
                    })
                .FirstOrDefaultAsync(e => e.Id == id);
            return Results.Ok(result);
        });
        app.MapPut("/participate/{eventId}", async (int eventId, [FromBody] Participant p) =>
        {
            await using var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<KinoContext>();

            var cinemaIds = p.VotedFor.Select(st => st.Cinema.Id).Distinct();
            foreach (var cinemaId in cinemaIds)
            {
                var existingCinema = await context.Cinemas.FindAsync(cinemaId);
                if (existingCinema == null)
                {
                    // This should only happen if you're sure you want to add new Cinemas
                    var cinemaName = p.VotedFor.FirstOrDefault(st => st.Cinema.Id == cinemaId)?.Cinema
                        .Name;
                    context.Cinemas.Add(new Cinema { Id = cinemaId, Name = cinemaName });
                }
                else
                {
                    // Attach existing cinemas to each showtime
                    foreach (var showtime in p.VotedFor.Where(st => st.Cinema.Id == cinemaId))
                    {
                        showtime.Cinema = existingCinema;
                    }
                }
            }

            foreach (var st in p.VotedFor)
            {
                var movieIds = p.VotedFor.Select(st => st.Movie.Id).Distinct();
                foreach (var movieId in movieIds)
                {
                    var existingMovie = await context.Movies.FindAsync(movieId);
                    if (existingMovie == null)
                    {
                        //find the correct movie in st
                        var movie = p.VotedFor.FirstOrDefault(st => st.Movie.Id == movieId)?.Movie;

                        // This should only happen if you're sure you want to add new Movies
                        context.Movies.Add(new Movie
                        {
                            Id = movieId, //insert all properties
                            AgeRating = movie.AgeRating,
                            Duration = movie.Duration,
                            ImageUrl = movie.ImageUrl,
                            Title = movie.Title,
                            PremiereDate = movie.PremiereDate
                        }); // Specify other properties
                    }
                    else
                    {
                        // Attach existing movies to each showtime
                        foreach (var showtime in p.VotedFor.Where(st => st.Movie.Id == movieId))
                        {
                            showtime.Movie = existingMovie;
                        }
                    }
                }

                var existingRoom = await context.Sals.FindAsync(st.Room.Id);
                if (existingRoom != null)
                {
                    context.Sals.Attach(existingRoom);
                    st.Room = existingRoom;
                }
                else
                {
                    context.Sals.Add(st.Room);
                }

                var existingVersionTag = await context.Versions.FirstOrDefaultAsync(v => v.Type == st.VersionTag.Type);
                if (existingVersionTag != null)
                {
                    Console.WriteLine("Existing version tag: " + existingVersionTag.Type);
                    context.Versions.Attach(existingVersionTag);
                    st.VersionTag = existingVersionTag;
                }
                else
                {
                    context.Versions.Add(st.VersionTag);
                }

                // Handle Playtime
                var existingPlaytime =
                    await context.Playtimes.FirstOrDefaultAsync(p => p.StartTime == st.Playtime.StartTime);
                if (existingPlaytime != null)
                {
                    context.Playtimes.Attach(existingPlaytime);
                    st.Playtime = existingPlaytime;
                }
                else
                {
                    context.Playtimes.Add(st.Playtime);
                }

                var existingShowtime = await context.Showtimes.FindAsync(st.Id);
                if (existingShowtime != null)
                {
                    context.Showtimes.Attach(existingShowtime);
                    st.Id = existingShowtime.Id;
                }
                else
                {
                    //add new showtime, with only the Ids of the related entities
                    var newShowtime = new Showtime
                    {
                        Id = st.Id,
                        MovieId = st.Movie.Id,
                        CinemaId = st.Cinema.Id,
                        PlaytimeId = st.Playtime.Id,
                        VersionTagId = st.VersionTag.Id,
                        RoomId = st.Room.Id
                    };

                    context.Showtimes.Add(newShowtime);
                }
            }

            var ShowtimesToAttach = new List<Showtime>();
            foreach (var showtime in p.VotedFor)
            {
                var existingShowtime = await context.Showtimes.FindAsync(showtime.Id);
                if (existingShowtime != null)
                {
                    ShowtimesToAttach.Add(existingShowtime);
                }
                else
                {
                    ShowtimesToAttach.Add(showtime);
                }
            }

            var participant = new Participant
            {
                Id = p.Id,
                JoinEventId = p.JoinEventId,
                Nickname = p.Nickname,
                VotedFor = ShowtimesToAttach
            };

            await context.Participants.AddAsync(participant);


            await context.SaveChangesAsync();
            var joinEvent = await context.JoinEvents.Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == eventId);
            if (joinEvent is { Participants: not null } && joinEvent.Participants.Exists(eP => eP.Id == p.Id))
            {
                joinEvent.Participants.Add(p);
            }

            await context.SaveChangesAsync();
            return Results.Ok();
        });

        app.MapPut("/putJoinEvent/", async ([FromBody] JoinEvent joinEvent) =>
        {
            await using var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<KinoContext>();
            //await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            foreach (var st in joinEvent.Showtimes)
            {
                var cinemaIds = joinEvent.Showtimes.Select(st => st.Cinema.Id).Distinct();
                foreach (var cinemaId in cinemaIds)
                {
                    var existingCinema = await context.Cinemas.FindAsync(cinemaId);
                    if (existingCinema == null)
                    {
                        // This should only happen if you're sure you want to add new Cinemas
                        var cinemaName = joinEvent.Showtimes.FirstOrDefault(st => st.Cinema.Id == cinemaId)?.Cinema
                            .Name;
                        context.Cinemas.Add(new Cinema { Id = cinemaId, Name = cinemaName });
                    }
                    else
                    {
                        // Attach existing cinemas to each showtime
                        foreach (var showtime in joinEvent.Showtimes.Where(st => st.Cinema.Id == cinemaId))
                        {
                            showtime.Cinema = existingCinema;
                        }
                    }
                }

                // Handle Playtime
                var existingPlaytime =
                    await context.Playtimes.FirstOrDefaultAsync(p => p.StartTime == st.Playtime.StartTime);
                if (existingPlaytime != null)
                {
                    context.Playtimes.Attach(existingPlaytime);
                    st.Playtime = existingPlaytime;
                }
                else
                {
                    context.Playtimes.Add(st.Playtime);
                }

                // Handle VersionTag
                var existingVersionTag = await context.Versions.FirstOrDefaultAsync(v => v.Type == st.VersionTag.Type);
                if (existingVersionTag != null)
                {
                    Console.WriteLine("Existing version tag: " + existingVersionTag.Type);
                    context.Versions.Attach(existingVersionTag);
                    st.VersionTag = existingVersionTag;
                }
                else
                {
                    context.Versions.Add(st.VersionTag);
                }

                // Handle Sal
                var existingSal = await context.Sals.FindAsync(st.Room.Id);
                if (existingSal != null)
                {
                    context.Sals.Attach(existingSal);
                    st.Room = existingSal;
                }
                else
                {
                    context.Sals.Add(st.Room);
                }

                await context.SaveChangesAsync();
            }

            //Handle movies Same way as cinemas
            foreach (var st in joinEvent.Showtimes)
            {
                var movieIds = joinEvent.Showtimes.Select(st => st.Movie.Id).Distinct();
                foreach (var movieId in movieIds)
                {
                    var existingMovie = await context.Movies.FindAsync(movieId);
                    if (existingMovie == null)
                    {
                        //find the correct movie in st
                        var movie = joinEvent.Showtimes.FirstOrDefault(st => st.Movie.Id == movieId)?.Movie;

                        // This should only happen if you're sure you want to add new Movies
                        context.Movies.Add(new Movie
                        {
                            Id = movieId, //insert all properties
                            AgeRating = movie.AgeRating,
                            Duration = movie.Duration,
                            ImageUrl = movie.ImageUrl,
                            Title = movie.Title,
                            PremiereDate = movie.PremiereDate
                        }); // Specify other properties
                    }
                    else
                    {
                        // Attach existing movies to each showtime
                        foreach (var showtime in joinEvent.Showtimes.Where(st => st.Movie.Id == movieId))
                        {
                            showtime.Movie = existingMovie;
                        }
                    }
                }
            }

            await context.SaveChangesAsync();

            //Handle showtimes
            foreach (var showtime in joinEvent.Showtimes)
            {
                var existingShowtime = await context.Showtimes.FindAsync(showtime.Id);
                if (existingShowtime != null)
                {
                    context.Showtimes.Attach(existingShowtime);
                    showtime.Id = existingShowtime.Id;
                }
                else
                {
                    //add new showtime, with only the Ids of the related entities
                    var newShowtime = new Showtime
                    {
                        Id = showtime.Id,
                        MovieId = showtime.Movie.Id,
                        CinemaId = showtime.Cinema.Id,
                        PlaytimeId = showtime.Playtime.Id,
                        VersionTagId = showtime.VersionTag.Id,
                        RoomId = showtime.Room.Id
                    };

                    context.Showtimes.Add(newShowtime);
                }
            }

            await context.SaveChangesAsync();

            // Handle Host
            var existingHost = await context.Hosts.FindAsync(joinEvent.Host.AuthId);
            if (existingHost != null)
            {
                context.Hosts.Attach(existingHost);
                joinEvent.Host = existingHost;
            }
            else
            {
                context.Hosts.Add(joinEvent.Host);
            }

            await context.SaveChangesAsync();

            //Get id of new host
            joinEvent.HostId = joinEvent.Host.AuthId;

            // Handle JoinEvent
            var newJoinEventId = 0;
            var existingJoinEvent = await context.JoinEvents.FindAsync(joinEvent.Id);
            if (existingJoinEvent != null)
            {
                context.JoinEvents.Attach(existingJoinEvent);
                joinEvent.Id = existingJoinEvent.Id;
                await context.SaveChangesAsync();
            }
            else
            {
                var ShowtimesToAttach = new List<Showtime>();
                foreach (var showtime in joinEvent.Showtimes)
                {
                    var existingShowtime = await context.Showtimes.FindAsync(showtime.Id);
                    if (existingShowtime != null)
                    {
                        ShowtimesToAttach.Add(existingShowtime);
                    }
                    else
                    {
                        ShowtimesToAttach.Add(showtime);
                    }
                }

                var newJoinEvent = new JoinEvent
                {
                    Id = joinEvent.Id,
                    Title = joinEvent.Title,
                    Description = joinEvent.Description,
                    Deadline = joinEvent.Deadline,
                    HostId = joinEvent.Host.AuthId,
                    Showtimes = ShowtimesToAttach
                };

                context.JoinEvents.Add(newJoinEvent);
                await context.SaveChangesAsync();
                newJoinEventId = newJoinEvent.Id;
            }

            //Get recently added joinEvent
            var recentlyAddedJoinEvent = await context.JoinEvents.Include(e => e.Showtimes)
                .FirstOrDefaultAsync(e => e.Id == newJoinEventId);

            //Confirm attributes
            Console.WriteLine("JoinEvent: " + recentlyAddedJoinEvent.Id);
            Console.WriteLine("Title: " + recentlyAddedJoinEvent.Title);
            Console.WriteLine(
                "movie of first showtime: " + recentlyAddedJoinEvent.Showtimes.FirstOrDefault().Movie.Title);

            return Results.Ok(newJoinEventId);
        });
    }
}
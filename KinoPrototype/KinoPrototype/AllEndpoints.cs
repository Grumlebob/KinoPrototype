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
        app.MapGet("/event/{id}", async (int id) =>
        {
            using var scope = app.Services.CreateScope();
            var result = await scope.ServiceProvider.GetRequiredService<KinoContext>().JoinEvents.Select(
                    e => new JoinEvent
                    {
                        Id = e.Id, Title = e.Title, Description = e.Description, Deadline = e.Deadline, Host = e.Host,
                        Showtimes = e.Showtimes.Select(s => new Showtime
                        {
                            Id = s.Id, Movie = s.Movie, Cinema = s.Cinema, Playtime = s.Playtime, Sal = s.Sal,
                            VersionTag = s.VersionTag
                        }).ToList()
                    })
                .FirstOrDefaultAsync(e => e.Id == id);
            return Results.Ok(result);
        });
        app.MapPut("/participate/{eventId}", async (int eventId, [FromBody] Participant p) =>
        {
            await using var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<KinoContext>();
            if (await context.Participants.ContainsAsync(p))
            {
                context.Participants.Update(p);
            }
            else
            {
                await context.Participants.AddAsync(p);
            }

            var joinEvent = await context.JoinEvents.Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == eventId);
            if (joinEvent != null && !joinEvent.Participants.Exists(eP => eP.Id == p.Id))
            {
                joinEvent.Participants.Add(p);
            }

            await context.SaveChangesAsync();
            return Results.Ok();
        });
        
        app.MapPut("/putJoinEvent/",async ([FromBody] JoinEvent joinEvent) =>
        {
            
            
            await using var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<KinoContext>();
            
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            foreach (var showtime in joinEvent.Showtimes)
            {
                var dbShowtime = await context.Showtimes.FirstOrDefaultAsync(showtime =>  showtime.Id == showtime.Id);
                
                if (dbShowtime != null)
                {
                    //joinEvent.Showtimes[joinEvent.Showtimes.IndexOf(showtime)] = dbShowtime;
                    continue;
                }
                
                //cinemas
                var dbCinema = await context.Cinemas.FirstOrDefaultAsync(cinema =>  showtime.Cinema.Navn == cinema.Navn);
                
                if (dbCinema != null)
                {
                   showtime.Cinema = dbCinema;
                }
                else
                {
                    await context.Cinemas.AddAsync(showtime.Cinema);
                }
                
                //Sals
                var dbSal = await context.Sals.FirstOrDefaultAsync(sal =>  showtime.Sal.Navn == sal.Navn);
                
                if (dbSal != null)
                {
                    showtime.Sal = dbSal;
                }
                else
                {
                    await context.Sals.AddAsync(showtime.Sal);
                }
                
                
                //Playtimes
                var dbPlaytime = await context.Playtimes.FirstOrDefaultAsync(playtime =>  showtime.Playtime.StartTime == playtime.StartTime);
                
                if (dbPlaytime != null)
                {
                    showtime.Playtime = dbPlaytime;
                }
                else
                {
                    await context.Playtimes.AddAsync(showtime.Playtime);
                }
                
                //VersionTags
                var dbVersionTag = await context.Versions.FirstOrDefaultAsync(version =>  showtime.VersionTag.Type == version.Type);
                
                if (dbVersionTag != null)
                {
                    showtime.VersionTag = dbVersionTag;
                }
                else
                {
                    await context.Versions.AddAsync(showtime.VersionTag);
                }
                
                context.SaveChanges();
                
                //Movies
                var dbMovie = await context.Movies.FirstOrDefaultAsync(movie =>  showtime.Movie.Navn == movie.Navn);
                
                if (dbMovie != null)
                {
                    showtime.Movie = dbMovie;
                }
                else
                {
                    await context.Movies.AddAsync(showtime.Movie);
                }
                
                context.SaveChanges();
            
                //Showtime
                await context.Showtimes.AddAsync(showtime);
                context.SaveChanges();
                
            }
            
            //Hosts
            var dbHost = await context.Hosts.FirstOrDefaultAsync(host =>  joinEvent.Host.Username == host.Username);
            
            if (dbHost != null)
            {
                joinEvent.Host = dbHost;
            }
            else
            {
                await context.Hosts.AddAsync(joinEvent.Host);
                context.SaveChanges();
            }
            
            
            //JoinEvent
            await context.JoinEvents.AddAsync(joinEvent);
            context.SaveChanges();
            
            return Results.Ok(joinEvent.Id);
        });
    }
}
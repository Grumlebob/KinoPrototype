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

        app.MapPut("/putJoinEvent/", async ([FromBody] JoinEvent joinEvent) =>
        {
            await using var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<KinoContext>();
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            
            foreach (var showtime in joinEvent.Showtimes)
            {
                // Handle Cinema
                var existingCinema = await context.Cinemas.FindAsync(showtime.Cinema.Id);
                if (existingCinema != null)
                {
                    context.Cinemas.Attach(existingCinema);
                    showtime.Cinema = existingCinema;
                }
                else
                {
                    context.Cinemas.Add(showtime.Cinema);
                }

                // Handle Movie
                //var existingMovie = await context.Movies.FindAsync(showtime.Movie.Id);
                //if (existingMovie != null)
                //{
                //    context.Movies.Attach(existingMovie);
                //    showtime.Movie = existingMovie;
                //}
                //else
                //{
                //    context.Movies.Add(showtime.Movie);
                //}

                // Handle Playtime
                var existingPlaytime = await context.Playtimes.FindAsync(showtime.Playtime.Id);
                if (existingPlaytime != null)
                {
                    context.Playtimes.Attach(existingPlaytime);
                    showtime.Playtime = existingPlaytime;
                }
                else
                {
                    context.Playtimes.Add(showtime.Playtime);
                }

                // Handle VersionTag
                var existingVersionTag = await context.Versions.FindAsync(showtime.VersionTag.Id);
                if (existingVersionTag != null)
                {
                    context.Versions.Attach(existingVersionTag);
                    showtime.VersionTag = existingVersionTag;
                }
                else
                {
                    context.Versions.Add(showtime.VersionTag);
                }

                // Handle Sal
                var existingSal = await context.Sals.FindAsync(showtime.Sal.Id);
                if (existingSal != null)
                {
                    context.Sals.Attach(existingSal);
                    showtime.Sal = existingSal;
                }
                else
                {
                    context.Sals.Add(showtime.Sal);
                }
            }

            await context.SaveChangesAsync();

            var joinEventToBeInserted = new JoinEvent
            {
                // Set properties for JoinEvent WITHOUT related entities such as Showtimes objects, only Ids
                Id = joinEvent.Id
            };
            
            
            //await context.SaveChangesAsync();
            return Results.Ok(123);
        });
    }
}
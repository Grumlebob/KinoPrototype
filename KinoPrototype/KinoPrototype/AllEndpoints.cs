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

            // Check if JoinEvent already exists
            var existingJoinEvent = await context.JoinEvents
                .Include(e => e.Showtimes)
                .ThenInclude(s => s.Movie)
                .FirstOrDefaultAsync(e => e.Id == joinEvent.Id);

            if (existingJoinEvent != null)
            {
                // Update existing JoinEvent
                context.Entry(existingJoinEvent).CurrentValues.SetValues(joinEvent);
            }
            else
            {
                // If JoinEvent does not exist, handle Showtimes and related entities before adding
                foreach (var showtime in joinEvent.Showtimes)
                {
                    
                }

                // After handling Showtimes and related entities, add the new JoinEvent
                await context.JoinEvents.AddAsync(joinEvent);
            }

            await context.SaveChangesAsync();
            return Results.Ok(joinEvent.Id);
        });
    }
}
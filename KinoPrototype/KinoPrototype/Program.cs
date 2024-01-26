using KinoPrototype;
using KinoPrototype.Client;
using KinoPrototype.Components;
using Microsoft.EntityFrameworkCore;
using Host = KinoPrototype.Host;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddDbContextFactory<KinoContext>(options =>
{
    var secret = builder.Configuration["PostgresConnection"];
    options.UseNpgsql(secret);
    options.EnableDetailedErrors();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(KinoPrototype.Client._Imports).Assembly);


app.MapApiEndpoints();


//RUN ONCE WHEN APP STARTS - Create a new scope to be able to resolve scoped services
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<KinoContext>();

    // Creating demo data
    var host = new Host { AuthId = "host1", Username = "DemoHost" };

    var movie = new Movie { Id = 1, Navn = "Demo Movie", Duration = 120, PremiereDate = DateTime.Now, AgeRating = 12, ImageUrl = "https://via.placeholder.com/150"};
    var cinema = new Cinema { Id = 1, Navn = "Demo Cinema" };
    var sal = new Sal { Id = 1, Navn = "Main Hall" };
    var playtime = new Playtime { Id = 1, StartTime = DateTime.Now.AddHours(2) };
    var version = new VersionTag { Id = 1, Type = "3D" };

    var showtime = new Showtime
    {
        MovieId = movie.Id,
        Movie = movie,
        CinemaId = cinema.Id,
        Cinema = cinema,
        PlaytimeId = playtime.Id,
        Playtime = playtime,
        VersionId = version.Id,
        VersionTag = version,
        SalId = sal.Id,
        Sal = sal
    };
    
    var participant1 = new Participant { Id = 1, Nickname = "Alice" };
    var participant2 = new Participant { Id = 2, Nickname = "Bob" };

    var joinEvent = new JoinEvent
    {
        Id = 1,
        Title = "Movie Night",
        Description = "Join us for a demo movie night!",
        Deadline = DateTime.Now.AddDays(1),
        Showtimes = new List<Showtime> { showtime },
        Participants = new List<Participant> { participant1, participant2 }
    };

    // Adding demo data to context
    dbContext.Hosts.Add(host);
    dbContext.Participants.AddRange(new[] { participant1, participant2 });
    dbContext.JoinEvents.Add(joinEvent);
    dbContext.Movies.Add(movie);
    dbContext.Showtimes.Add(showtime);
    dbContext.Playtimes.Add(playtime);
    dbContext.Versions.Add(version);
    dbContext.Cinemas.Add(cinema);
    dbContext.Sals.Add(sal);

    // Save changes to the in-memory database
    dbContext.SaveChanges();
    
    //Make the demo data vote for a specfic movie in the join event
    movie.Showtimes.Add(showtime);
    

    // Demo: Displaying the details of the join event
    Console.WriteLine($"Event: {joinEvent.Title}");
    Console.WriteLine($"Host: {host.Username}");
    Console.WriteLine($"Participants: {string.Join(", ", joinEvent.Participants.Select(p => p.Nickname))}");
    Console.WriteLine($"Movie: {showtime.Movie.Navn} at {showtime.Playtime.StartTime}");
}

app.Run();
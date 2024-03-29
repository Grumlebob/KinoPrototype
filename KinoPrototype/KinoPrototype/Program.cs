
using KinoPrototype;
using KinoPrototype.Components;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//use Kestrel
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Listen on port 5000 for HTTP
    serverOptions.ListenLocalhost(5000);

    // Listen on port 5001 for HTTPS
    serverOptions.ListenLocalhost(5001, listenOptions => { listenOptions.UseHttps(); });

    // Example: Set limits on request headers and body size
    serverOptions.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
    serverOptions.Limits.MaxRequestHeaderCount = 50;

    // Example: Set a keep-alive timeout
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin() // This allows requests from any origin
                .AllowAnyHeader()
                .AllowAnyMethod();
            // Do not call AllowCredentials() when using AllowAnyOrigin()
        });
});


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

app.UseCors("AllowLocalhostDevelopment");

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

//app.UseHttpsRedirection(); //Disable when hosting, because we haven't setup certificates to handle hosting on HTTPS, only HTTP

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

    /*
    await dbContext.Database.EnsureDeletedAsync();


    await dbContext.Database.EnsureCreatedAsync();

   // Creating demo data

   // has no depencies
   var playtime = new Playtime { Id = 1, StartTime = DateTime.Now.AddHours(2) };
   var version = new VersionTag { Id = 1, Type = "3D" };
   var cinema = new KinoPrototype.Domain.Cinema { Id = 1, Navn = "Demo Cinema" };
   var sal = new Sal { Id = 1, Navn = "Main Hall" };

   // deps on showTime
   var movie = new KinoPrototype.Domain.Movie { Id = 1, Navn = "Demo Movie", Duration = 120, PremiereDate = DateTime.Now.ToString(), AgeRating = "12", ImageUrl = "https://via.placeholder.com/150"};

   // deps on joinEvent
   var host = new KinoPrototype.Domain.Host { AuthId = "host1", Username = "DemoHost" };

   // deps on everything
   var showtime = new KinoPrototype.Domain.Showtime
   {
       MovieId = movie.Id,
       Movie = movie,
       CinemaId = cinema.Id,
       Cinema = cinema,
       PlaytimeId = playtime.Id,
       Playtime = playtime,
       VersionTagId = version.Id,
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
       Showtimes = new List<KinoPrototype.Domain.Showtime> { showtime },
       Participants = new List<Participant> { participant1, participant2 },
   };
   // Adding demo data to context

   dbContext.Playtimes.Add(playtime);
   dbContext.Versions.Add(version);
   dbContext.Cinemas.Add(cinema);
   dbContext.Sals.Add(sal);

   dbContext.SaveChanges();

   dbContext.Hosts.Add(host);
   dbContext.Participants.AddRange(new[] { participant1, participant2 });
   dbContext.JoinEvents.Add(joinEvent);
   dbContext.Movies.Add(movie);

   // Save changes to the in-memory database
   dbContext.SaveChanges();


   //Make the demo data vote for a specfic movie in the join event


   // Prints results from reading the database
   Console.WriteLine($"There are {dbContext.JoinEvents.Count()} join events in the database");
   Console.WriteLine($"There are {dbContext.Movies.Count()} movies in the database");
   Console.WriteLine($"There are {dbContext.Participants.Count()} participants in the database");
   Console.WriteLine($"There are {dbContext.Hosts.Count()} hosts in the database");
   Console.WriteLine($"There are {dbContext.Showtimes.Count()} showtimes in the database");
   Console.WriteLine($"There are {dbContext.Playtimes.Count()} playtimes in the database");
   Console.WriteLine($"There are {dbContext.Versions.Count()} versions in the database");
   Console.WriteLine($"There are {dbContext.Cinemas.Count()} cinemas in the database");
   Console.WriteLine($"There are {dbContext.Sals.Count()} sals in the database");

   //var jsonResult = JsonParser.GetMoviesFromJson();
   */
}


app.Run();
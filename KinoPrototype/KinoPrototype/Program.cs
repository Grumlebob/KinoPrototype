using KinoPrototype;
using KinoPrototype.Client;
using KinoPrototype.Components;
using Microsoft.EntityFrameworkCore;
using Host = KinoPrototype.Host;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddDbContextFactory<TheContext>(options =>
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
    var dbContext = scope.ServiceProvider.GetRequiredService<TheContext>();

    //add demo user to MovieContext
    var demoUser = new Host()
    {
        Id = 1,
        Username = "demo",
    };
    dbContext.Hosts.Add(demoUser);
    await dbContext.SaveChangesAsync();
    
    //print all hosts
    var hosts = await dbContext.Hosts.ToListAsync();
    foreach (var host in hosts)
    {
        Console.WriteLine("user"+ host.Username);
    }
}

app.Run();
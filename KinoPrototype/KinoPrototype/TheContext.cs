using Microsoft.EntityFrameworkCore;

namespace KinoPrototype;

public class TheContext: DbContext
{
    public DbSet<User> users { get; set; }

    public TheContext(DbContextOptions<TheContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        


    }
}

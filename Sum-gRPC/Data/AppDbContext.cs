using Microsoft.EntityFrameworkCore;
using Sum_gRPC.Models;

namespace Sum_gRPC.Data;

public class AppDbContext : DbContext
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<OutboxMessage>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Content).IsRequired();
        });
    }
}

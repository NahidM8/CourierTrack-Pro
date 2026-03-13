using CourierTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierTrack.Infrastructure.Data.Context;

public class CourierTrackDbContext : DbContext
{
    public CourierTrackDbContext(DbContextOptions<CourierTrackDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    //public DbSet<Courier> Couriers { get; set; }
    //public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CourierTrackDbContext).Assembly);
    }
}
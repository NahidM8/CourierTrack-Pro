using CourierTrack.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CourierTrack.Infrastructure.Data.Context;

public class CourierTrackDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public CourierTrackDbContext(DbContextOptions<CourierTrackDbContext> options)
        : base(options)
    {
    }

    public DbSet<Courier> Couriers { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CourierTrackDbContext).Assembly);
    }
}
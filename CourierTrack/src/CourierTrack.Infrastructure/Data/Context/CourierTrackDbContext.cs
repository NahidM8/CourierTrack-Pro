namespace CourierTrack.Infrastructure.Data.Context;

public class CourierTrackDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public CourierTrackDbContext(DbContextOptions<CourierTrackDbContext> options)
        : base(options)
    {
    }

    public DbSet<Courier> Couriers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CourierTrackDbContext).Assembly);
    }
}
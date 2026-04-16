using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourierTrack.Infrastructure.Configurations;

public class CourierConfiguration : IEntityTypeConfiguration<Courier>
{
    public void Configure(EntityTypeBuilder<Courier> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.VehicleType)
            .IsRequired();

        builder.Property(c => c.IsAvailable)
            .IsRequired();

        builder.Property(c => c.Rating)
            .HasPrecision(3, 2);

        builder.Property(c => c.TotalDeliveries)
            .IsRequired();
    }
}
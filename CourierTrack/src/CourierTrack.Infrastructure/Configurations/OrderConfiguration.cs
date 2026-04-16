using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourierTrack.Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.CourierId)
            .IsRequired(false);

        builder.Property(o => o.TrackingNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(o => o.TrackingNumber)
            .IsUnique();

        builder.Property(o => o.PickupAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.PickupLatitude)
            .IsRequired();

        builder.Property(o => o.PickupLongitude)
            .IsRequired();

        builder.Property(o => o.DeliveryAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.DeliveryLatitude)
            .IsRequired();

        builder.Property(o => o.DeliveryLongitude)
            .IsRequired();

        builder.Property(o => o.PackageDescription)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(o => o.PackageWeight)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(o => o.PackageSize)
            .IsRequired();

        builder.Property(o => o.EstimatedDistanceKm)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(o => o.EstimatedDuration)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Price)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(o => o.Status)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.PickedUpAt)
            .IsRequired(false);

        builder.Property(o => o.DeliveredAt)
            .IsRequired(false);

        builder.HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Courier)
            .WithMany()
            .HasForeignKey(o => o.CourierId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

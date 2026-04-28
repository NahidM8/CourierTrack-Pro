using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourierTrack.Infrastructure.Configurations;

public class CourierRatingConfiguration : IEntityTypeConfiguration<CourierRating>
{
    public void Configure(EntityTypeBuilder<CourierRating> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd();

        builder.Property(r => r.OrderId)
            .IsRequired();

        builder.Property(r => r.CourierId)
            .IsRequired();

        builder.Property(r => r.CustomerId)
            .IsRequired();

        builder.Property(r => r.Score)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.HasOne(r => r.Courier)
            .WithMany()
            .HasForeignKey(r => r.CourierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Order)
            .WithMany()
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

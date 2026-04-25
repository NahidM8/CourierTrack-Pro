using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourierTrack.Infrastructure.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OldStatus)
            .HasConversion<string>()
            .IsRequired(false);

        builder.Property(o => o.NewStatus)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(o => o.Note)
            .HasMaxLength(500);

        builder.Property(o => o.ChangedAt)
            .IsRequired();

        builder.Property(o => o.ChangedBy)
            .IsRequired();

        builder.HasOne(o => o.Order)
            .WithMany(o => o.StatusHistory)
            .HasForeignKey(o => o.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

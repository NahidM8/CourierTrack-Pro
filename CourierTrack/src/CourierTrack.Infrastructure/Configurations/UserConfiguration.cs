using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourierTrack.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(u => u.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.Property(u => u.Role)
            .IsRequired();
        
        builder.Property(u => u.CreatedAt)
            .IsRequired();
        
        builder.Property(u => u.IsActive)
            .IsRequired();

        builder.HasOne(u => u.Courier)
            .WithOne(c => c.User)
            .HasForeignKey<Courier>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

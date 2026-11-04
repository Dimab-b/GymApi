using Gym.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.OwnsOne(p => p.Amount, amountBuilder =>
        {
            amountBuilder.Property(a => a.Value)
                .HasColumnName("AmountValue")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            amountBuilder.Property(a => a.Currency)
                .HasColumnName("AmountCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(p => p.MemberId).IsRequired();
        builder.Property(p => p.SubscriptionId);
        builder.Property(p => p.PaidAt).IsRequired();

        builder.Property(p => p.Method)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.Purpose)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.Note).HasMaxLength(500);

        builder.HasIndex(p => p.MemberId);
        builder.HasIndex(p => p.SubscriptionId);
        builder.HasIndex(p => p.PaidAt);
    }
}

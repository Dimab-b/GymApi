using Gym.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.OwnsOne(s => s.Price, priceBuilder =>
        {
            priceBuilder.Property(p => p.Value)
                .HasColumnName("PriceAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            priceBuilder.Property(p => p.Currency)
                .HasColumnName("PriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(s => s.StartDate).IsRequired();
        builder.Property(s => s.EndDate).IsRequired();
    }
}
using Gym.Domain.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Persistance.Configurations
{
    public class SessionBookingConfiguration : IEntityTypeConfiguration<SessionBooking>
    {
        public void Configure(EntityTypeBuilder<SessionBooking> builder)
        {
            builder.ToTable("Bookings");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.MemberId)
                .IsRequired();

            builder.Property(b => b.TrainerId)
                .IsRequired();

            builder.Property(b => b.StartTime)
                .IsRequired();

            builder.Property(b => b.EndTime)
                .IsRequired();

            builder.Property(b => b.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.OwnsOne(b => b.FixedTrainerPrice, priceBuilder =>
            {
                priceBuilder.Property(p => p.Value)
                    .HasColumnName("FixedPrice_Value")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                priceBuilder.Property(p => p.Currency)
                    .HasColumnName("FixedPrice_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            builder.HasIndex(b => new { b.TrainerId, b.StartTime, b.EndTime });
        }
    }
}

using Gym.Domain.Trainers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Persistance.Configurations
{
    internal class TrainerConfiguration : IEntityTypeConfiguration<Trainer>
    {
        public void Configure(EntityTypeBuilder<Trainer> builder)
        {
            builder.ToTable("Trainers");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Specialization)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.IsActive)
                .IsRequired();

            builder.OwnsOne(t => t.Email, emailBuilder =>
            {
                emailBuilder.Property(e => e.Value)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(255);

                emailBuilder.HasIndex(e => e.Value).IsUnique();
            });

            builder.OwnsOne(t => t.SessionPrice, priceBuilder =>
            {
                priceBuilder.Property(p => p.Value)
                    .HasColumnName("SessionPrice_Value")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                priceBuilder.Property(p => p.Currency)
                    .HasColumnName("SessionPrice_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });
        }
    }
}

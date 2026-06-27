using Gym.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Text;
using Gym.Domain.Members.Value_Objects;
namespace Gym.Infrastructure.Persistance.Configurations
{
    internal class MemberConfiguration : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> builder)
        {
          
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(100);

            
            builder.OwnsOne(m => m.Email, emailBuilder =>
            {
                emailBuilder.Property(e => e.Value)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(255);

                emailBuilder.HasIndex(e => e.Value).IsUnique();
            });

          
            builder.OwnsOne(m => m.BodyMetrics, metricsBuilder =>
            {
                metricsBuilder.Property(b => b.HeightCm).HasColumnName("HeightCm").HasColumnType("decimal(5,2)");
                metricsBuilder.Property(b => b.WeightKg).HasColumnName("WeightKg").HasColumnType("decimal(5,2)");
                metricsBuilder.Property(b => b.Age).HasColumnName("Age");
                metricsBuilder.Property(b => b.Goal).HasColumnName("Goal").HasMaxLength(500);
            });

           
            builder.HasMany(m => m.Subscriptions)
                .WithOne() 
                .HasForeignKey("MemberId") 
                .OnDelete(DeleteBehavior.Cascade);

        
            builder.Navigation(m => m.Subscriptions)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}

using Gym.Domain.Bookings;
using Gym.Domain.Common;
using Gym.Domain.Members;
using Gym.Domain.Trainers;
using Microsoft.EntityFrameworkCore;

namespace Gym.Infrastructure.Persistence
{
    public class AppDbContext : DbContext , IUnitOfWork
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Member> Members { get; set; } = null!;
        public DbSet<Trainer> Trainers { get; set; } = null!;
        public DbSet<SessionBooking> Bookings { get; set; } = null!;
        public DbSet<Subscription> Subscriptions {  get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}

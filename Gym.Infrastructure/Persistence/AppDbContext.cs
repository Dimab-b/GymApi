using Gym.Domain.Bookings;
using Gym.Domain.Common;
using Gym.Domain.Members;
using Gym.Domain.Payments;
using Gym.Domain.Trainers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Gym.Infrastructure.Persistence
{
    public class AppDbContext : DbContext , IUnitOfWork
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Member> Members { get; set; } = null!;
        public DbSet<Trainer> Trainers { get; set; } = null!;
        public DbSet<SessionBooking> Bookings { get; set; } = null!;
        public DbSet<Subscription> Subscriptions {  get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (
                ex.InnerException is PostgresException pg &&
                pg.SqlState == PostgresErrorCodes.ExclusionViolation &&
                pg.TableName == "Subscriptions" &&
                pg.ConstraintName == "EX_Subscriptions_MemberId_Period")
            {
                throw new SubscriptionConflictException(
                    "A subscription period for this member overlaps an existing period.");
            }
        }
    }
}

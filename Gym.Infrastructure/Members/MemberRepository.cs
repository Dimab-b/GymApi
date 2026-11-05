using Gym.Domain.Common.VO;
using Gym.Domain.Members;
using Gym.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Members
{
    public class MemberRepository : IMemberRepository
    {
        private readonly AppDbContext _context;
        public MemberRepository(AppDbContext context) 
        {
            _context = context;
        }

        public async Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Members.Include(x => x.Subscriptions).FirstOrDefaultAsync(x=> x.Id == id , cancellationToken);
        }

        public async Task<Member?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
        {
            return await _context.Members.Include(x => x.Subscriptions).FirstOrDefaultAsync(x=> x.Email.Value == email.Value , cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
        {
            return await _context.Members.AnyAsync(x => x.Email.Value == email.Value, cancellationToken);
        }

        public async Task LockForPurchaseAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT 1 FROM \"Members\" WHERE \"Id\" = {id} FOR UPDATE NOWAIT",
                    cancellationToken);
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.LockNotAvailable)
            {
                throw new SubscriptionConflictException(
                    "Another operation is already in progress for this member.",
                    ex);
            }
        }

        public async Task LockForBanAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "SET LOCAL lock_timeout = '5s'",
                cancellationToken);

            try
            {
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT 1 FROM \"Members\" WHERE \"Id\" = {id} FOR UPDATE",
                    cancellationToken);
            }
            catch (PostgresException ex) when (
                ex.SqlState == PostgresErrorCodes.LockNotAvailable ||
                ex.SqlState == PostgresErrorCodes.QueryCanceled)
            {
                throw new SubscriptionConflictException(
                    "Another operation is already in progress for this member.",
                    ex);
            }
        }

        public void Add(Member member)
        {
            _context.Members.Add(member);
        }

        public void Update(Member member)
        {
            _context.Members.Update(member);
        }
    }
}

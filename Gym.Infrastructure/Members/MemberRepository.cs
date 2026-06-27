using Gym.Domain.Members;
using Gym.Domain.Members.Value_Objects;
using Gym.Infrastructure.Persistance;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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

using Gym.Domain.Members.Value_Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Domain.Members
{
    public interface IMemberRepository
    {
        Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Member?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);
        void Add(Member member);
        void Update(Member member);
    }
}


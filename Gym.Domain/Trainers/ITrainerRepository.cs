using Gym.Domain.Common.VO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Domain.Trainers
{
    public interface ITrainerRepository
    {
        Task<Trainer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        void Add(Trainer trainer);
        void Update(Trainer trainer);
        Task<IEnumerable<Trainer?>> GetBySpecialization(string Specialization , CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);
    }
}

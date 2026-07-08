using Gym.Domain.Members;
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
        Task<Trainer?> GetBySpecialization(string Specialization);
    }
}

using Gym.Domain.Trainers;
using Gym.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Trainers
{
    public class TrainerRepository : ITrainerRepository
    {
        private readonly AppDbContext _context;
        public TrainerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Trainer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Trainers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }


        public void Add(Trainer trainer)
        {
            _context.Trainers.Add(trainer);
        }

        public void Update(Trainer trainer)
        {
            _context.Trainers.Update(trainer);
        }

        public async Task<Trainer?> GetBySpecialization(string Specialization)
        {
            return await _context.Trainers.Where(x => EF.Functions.Like(x.Specialization, $"%%{Specialization}%%")).ToListAsync();
        }
    }
}

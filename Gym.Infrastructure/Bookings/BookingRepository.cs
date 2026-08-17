using Gym.Domain.Bookings;
using Gym.Domain.Trainers;
using Gym.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Bookings
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;
        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }
        public void Add(SessionBooking booking)
        {
            _context.Bookings.Add(booking);
        }

        public void Update(SessionBooking booking)
        {
            _context.Bookings.Update(booking);
        }
        public async Task<SessionBooking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Bookings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
        public async Task<IEnumerable<SessionBooking>> GetByTrainerId(Guid TrainerId, CancellationToken cancellationToken = default)
        {
            return await _context.Bookings.Where(x => x.TrainerId == TrainerId
                 && x.Status == BookingStatus.Scheduled
                 && x.StartTime >= DateTime.UtcNow).ToListAsync(cancellationToken);
        }
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

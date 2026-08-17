using Gym.Domain.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Domain.Bookings
{
    public interface IBookingRepository
    {
        Task<SessionBooking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        void Add(SessionBooking booking);
        void Update(SessionBooking booking);
        Task<IEnumerable<SessionBooking>> GetByTrainerId(Guid TrainerId , CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

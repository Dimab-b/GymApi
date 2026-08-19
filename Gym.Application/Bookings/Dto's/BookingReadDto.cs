using Gym.Domain.Bookings;
using Gym.Domain.Common.VO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Bookings.Dto_s
{
    public record BookingReadDto
    (
    Guid Id,
    Guid TrainerId,
    string TrainerName,
    string TrainerSpecialization,
    DateTime StartTime,
    DateTime EndTime,
    string Status,
    decimal PriceAmount,
    string PriceCurrency
    );
}

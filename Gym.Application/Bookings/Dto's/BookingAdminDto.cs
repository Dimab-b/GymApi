using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Bookings.Dto_s
{
    public record BookingAdminDto(
        Guid Id,
        Guid MemberId,
        string MemberName,
        string MemberEmail,
        Guid TrainerId,
        string TrainerName,
        string TrainerSpecialization,
        DateTime StartTime,
        DateTime EndTime,
        string Status,
        decimal PriceAmount,
        string PriceCurrency);
}

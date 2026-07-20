using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Trainers.Dto_s
{
    public record TrainerAdminDetailsDto(
        Guid Id,
        string Name,
        string Specialization,
        decimal CurrentPrice,
        string Currency,
        TrainerStatsDto Stats,
        List<TrainerBookingDto> RecentBookings);
}

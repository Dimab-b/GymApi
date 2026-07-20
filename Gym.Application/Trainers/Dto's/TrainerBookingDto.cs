using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Trainers.Dto_s
{
    public record TrainerBookingDto(
        Guid Id,
        DateTime StartTime,
        string Status,
        string ClientName);
}

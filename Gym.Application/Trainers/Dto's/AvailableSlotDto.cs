using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Trainers.Dto_s
{
    public record AvailableSlotDto(
    TimeSpan StartTime,
    TimeSpan EndTime
    );
}

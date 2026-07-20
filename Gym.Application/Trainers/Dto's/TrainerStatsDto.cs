using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Trainers.Dto_s
{
    public record TrainerStatsDto(
        int TotalSessionsCompleted,
        decimal TotalEarned,
        int ActiveClientsCount);
}

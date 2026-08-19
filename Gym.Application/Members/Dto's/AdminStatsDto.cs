using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Members.Dto_s
{
    public record AdminStatsDto
    {
        public int CountActiveSubs { get; set; }
        public decimal TotalProfitLastMonth { get; set; }
    }
}

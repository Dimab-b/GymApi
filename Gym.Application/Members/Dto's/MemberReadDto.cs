using Gym.Domain.Members;
using Gym.Domain.Members.Value_Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Members.Dto_s
{
    public class MemberReadDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool IsBanned { get; set; }


        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }
        public int? Age { get; set; }
        public string? Goal { get; set; }


        public List<SubscriptionReadDto> Subscriptions { get; set; } = new();
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Members.Dto_s
{
    public class SubscriptionReadDto
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PriceAmount { get; set; }
        public string PriceCurrency { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}

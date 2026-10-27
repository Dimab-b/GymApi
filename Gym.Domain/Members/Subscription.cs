using Gym.Domain.Common;
using Gym.Domain.Common.VO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Gym.Domain.Members
{
    public class Subscription : Entity
    {
        public Guid Id { get; private set; }
        public string PlanCode { get; private set; }
        public int? VisitLimitPerMonth { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public Price Price { get; private set; }


        public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;

        private Subscription() { }


        internal Subscription(Guid id, string planCode, int? visitLimitPerMonth, DateTime startDate, int durationMonths, Price totalPrice)
        {
            Id = id;
            PlanCode = planCode;
            VisitLimitPerMonth = visitLimitPerMonth;
            StartDate = startDate;
            EndDate = startDate.AddMonths(durationMonths);
            Price = totalPrice;
        }

        internal void Cancel()
        {
            var now = DateTime.UtcNow;

            if (EndDate < now)
                return;

            EndDate = now > StartDate ? now : StartDate;
        }
    }
}

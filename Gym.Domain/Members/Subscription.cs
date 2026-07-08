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
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public Price Price { get; private set; }


        public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;

        private Subscription() { }


        internal Subscription(Guid id, int durationMonths, Price price)
        {
            Id = id;
            StartDate = DateTime.UtcNow;
            EndDate = StartDate.AddMonths(durationMonths);
            Price = price;
        }

        internal void Extend(int extraMonths)
        {
            EndDate = EndDate.AddMonths(extraMonths);
        }

        internal void Cancel()
        {
            if (EndDate < DateTime.UtcNow)
                return;

            
            EndDate = DateTime.UtcNow;
        }
    }
}

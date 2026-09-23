using Gym.Domain.Common;
using Gym.Domain.Common.VO;
using Gym.Domain.Members.Value_Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Domain.Members
{
    public class Member : Entity , IAggregateRoot
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public Email Email { get; private set; }
        public BodyMetrics? BodyMetrics { get; private set; }
        public bool IsBanned { get; private set; }

        private readonly List<Subscription> _subscriptions = new();
        public IReadOnlyCollection<Subscription> Subscriptions => _subscriptions.AsReadOnly();


        private Member() { }
        private Member(Guid id ,string name , Email email)
        {
            Id = id;
            Name = name;
            Email = email;
            IsBanned = false;
        }

        public static Member Create(string name , Email email)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name must be filled", nameof(name));


            var member = new Member(Guid.NewGuid() , name , email);

            member.AddDomainEvent(new MemberCreatedEvent(member.Id, email.Value));

            return member;
        }


        public bool HasActiveSubscription()
            => _subscriptions.Any(s => s.IsActive);


        public void PurchaseSubscription(int durationMonths, Price monthlyPrice)
        {
            if (durationMonths < 1)
                throw new ArgumentException("Duration of subscription must be greater than 0");

            if (IsBanned)
                throw new InvalidOperationException("Banned members cannot purchase subscriptions.");

            if (_subscriptions.Any(s => s.IsActive))
                throw new InvalidOperationException("Member already has an active subscription.");

            var totalPrice = new Price(monthlyPrice.Value * durationMonths, monthlyPrice.Currency);

            var newSub = new Subscription(Guid.NewGuid(), durationMonths, totalPrice);

            _subscriptions.Add(newSub);

            AddDomainEvent(new SubscriptionPurchasedEvent(this.Id, newSub.Id , newSub.StartDate , newSub.EndDate));
        }

        public void UpdateBodyMetrics(decimal height, decimal weight, int age, string goal)
        {
            if (this.IsBanned)
            {
                throw new InvalidOperationException("Banned members cannot update body-metrics");
            }
            BodyMetrics = new BodyMetrics(height, weight, age, goal);

            AddDomainEvent(new BodyMetricsUpdatedEvent(Id, weight, height));
        }


        public void ExtendActiveSubscription(int extraMonths)
        {
            if (IsBanned)
                throw new InvalidOperationException("Banned members cannot extend subscription");

            var activeSub = _subscriptions.FirstOrDefault(s => s.IsActive);

            if (activeSub == null)
                throw new InvalidOperationException("Member does not have an active subscription to extend.");

            activeSub.Extend(extraMonths);
        }


        public void BanUser()
        {
            this.IsBanned = true;


            var activeSubscriptions = _subscriptions.Where(s => s.IsActive).ToList();

            
            foreach (var sub in activeSubscriptions)
            {
                sub.Cancel();
            }

        }
    }
}

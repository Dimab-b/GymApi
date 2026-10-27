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

            member.AddDomainEvent(new MemberCreatedDomainEvent(email.Value));

            return member;
        }


        public bool HasActiveSubscription()
            => _subscriptions.Any(s => s.IsActive);


        public Subscription? ActiveSubscription
            => _subscriptions.FirstOrDefault(s => s.IsActive);


        public void PurchaseSubscription(string planCode, decimal pricePerMonth, string currency, int durationMonths, int? visitLimitPerMonth)
        {
            if (durationMonths < 1)
                throw new ArgumentException("Duration of subscription must be greater than 0");

            if (IsBanned)
                throw new InvalidOperationException("Banned members cannot purchase subscriptions.");

            var startDate = GetNextPeriodStart(DateTime.UtcNow);
            var totalPrice = new Price(pricePerMonth * durationMonths, currency);

            var newSub = new Subscription(Guid.NewGuid(), planCode, visitLimitPerMonth, startDate, durationMonths, totalPrice);

            _subscriptions.Add(newSub);

            AddDomainEvent(new SubscriptionPurchasedDomainEvent(this.Id, newSub.Id , newSub.StartDate , newSub.EndDate , Name , Email.Value));
        }

        public void PurchaseSubscription(int durationMonths, Price monthlyPrice)
            => PurchaseSubscription("legacy", monthlyPrice.Value, monthlyPrice.Currency, durationMonths, null);

        private DateTime GetNextPeriodStart(DateTime nowUtc)
        {
            var latestEnd = _subscriptions
                .Select(s => s.EndDate)
                .DefaultIfEmpty(nowUtc)
                .Max();

            return latestEnd > nowUtc ? latestEnd : nowUtc;
        }

        public void UpdateBodyMetrics(decimal height, decimal weight, int age, string goal)
        {
            if (this.IsBanned)
            {
                throw new InvalidOperationException("Banned members cannot update body-metrics");
            }
            BodyMetrics = new BodyMetrics(height, weight, age, goal);

            AddDomainEvent(new BodyMetricsUpdatedDomainEvent(Id, weight, height));
        }


        public void BanUser()
        {
            if (IsBanned)
                return;

            this.IsBanned = true;


            var subscriptionsToCancel = _subscriptions
                .Where(s => s.EndDate > DateTime.UtcNow)
                .ToList();

            foreach (var sub in subscriptionsToCancel)
            {
                sub.Cancel();
            }

            AddDomainEvent(new MemberBannedDomainEvent(this.Id, Name, Email.Value));
        }
    }
}

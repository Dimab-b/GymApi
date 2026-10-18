using Gym.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Domain.Members
{
    public record MemberCreatedDomainEvent(string Email) : IDomainEvent;
    public record SubscriptionPurchasedDomainEvent(Guid MemberId, Guid SubscriptionId , DateTime StartDate , DateTime EndDate , string Name , string Email) : IDomainEvent;
    public record BodyMetricsUpdatedDomainEvent(Guid MemberId, decimal WeightKg, decimal HeightCm) : IDomainEvent;
    public record MemberBannedDomainEvent(Guid MemberId, string Name, string Email) : IDomainEvent;

}

using Gym.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Domain.Members
{
    public record MemberCreatedEvent(Guid MemberId, string Email) : IDomainEvent;
    public record SubscriptionPurchasedEvent(Guid MemberId, Guid SubscriptionId) : IDomainEvent;
    public record BodyMetricsUpdatedEvent(Guid MemberId, decimal WeightKg, decimal HeightCm) : IDomainEvent;
    public record MemberBannedEvent(Guid MemberId) : IDomainEvent;

}

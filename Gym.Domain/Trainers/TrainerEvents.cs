using Gym.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Domain.Trainers
{
    public record TrainerCreatedDomainEvent(string Name , string Email) : IDomainEvent;
    public record TrainerDeactivatedDomainEvent(Guid TrainerId) : IDomainEvent;
    public record TrainerChangedPriceDomainEvent(Guid TrainerId) : IDomainEvent;
    
}

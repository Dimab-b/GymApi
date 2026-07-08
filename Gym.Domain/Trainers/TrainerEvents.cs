using Gym.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Domain.Trainers
{
    public record TrainerCreatedEvent() : IDomainEvent;
    public record TrainerDeactivatedEvent() : IDomainEvent;
    
}

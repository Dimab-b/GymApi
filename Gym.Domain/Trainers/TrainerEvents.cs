using FluentEmail.Core;
using Gym.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Domain.Trainers
{
    public record TrainerCreatedEvent(string Name , string Email) : IDomainEvent;
    public record TrainerDeactivatedEvent(Guid TrainerId) : IDomainEvent;
    
}

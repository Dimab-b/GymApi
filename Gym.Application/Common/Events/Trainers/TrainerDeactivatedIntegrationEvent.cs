using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Common.Events.Trainers
{
    public record TrainerDeactivatedIntegrationEvent(Guid TrainerId);
}
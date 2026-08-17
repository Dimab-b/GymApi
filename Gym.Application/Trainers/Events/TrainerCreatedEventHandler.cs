using Gym.Application.Common.Interfaces;
using Gym.Domain.Trainers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Trainers.Events
{
    public class TrainerCreatedEventHandler : INotificationHandler<TrainerCreatedEvent>
    {
        private readonly IEmailSender _emailSender;
        private readonly ITrainerRepository _trainerRepository;

        public TrainerCreatedEventHandler(IEmailSender emailSender , ITrainerRepository trainerRepository)
        {
            _emailSender = emailSender;
            _trainerRepository = trainerRepository;
        }
        public async Task Handle(TrainerCreatedEvent createdEvent , CancellationToken cancellationToken = default)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Congratulations with joining to our gym");
            sb.AppendLine("We look forward to long-term cooperation");
            string body = sb.ToString();
            await _emailSender.SendAsync(createdEvent.Email, $"{createdEvent.Name} , You You can acquire clients.", body);
        }
    }
}

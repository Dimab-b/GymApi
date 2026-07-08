using Gym.Domain.Common;
using Gym.Domain.Common.VO;
using Gym.Domain.Members;
using Gym.Domain.Members.Value_Objects;
using Gym.Domain.Trainers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Trainers.Commands
{
    public record CreateTrainerCommand(string Name , string Email , string Specialization , decimal SessionPrice , string Currency) : IRequest<Guid>;


    public class CreateTrainerCommandHandler : IRequestHandler<CreateTrainerCommand , Guid>
    {
        private readonly ITrainerRepository _trainerRepository;
        private readonly IUnitOfWork _uow;

        public CreateTrainerCommandHandler(ITrainerRepository trainerRepository , IUnitOfWork uow)
        {
            _trainerRepository = trainerRepository;
            _uow = uow;
        }

        public async Task<Guid> Handle(CreateTrainerCommand command, CancellationToken cancellationToken = default)
        {
            var validEmail = new Email(command.Email);
            var validPrice = new Price(command.SessionPrice, command.Currency);

            var emailExists = await _trainerRepository.ExistsByEmailAsync(validEmail, cancellationToken);
            if (emailExists)
                throw new InvalidOperationException("Trainer with this email already exists.");

            var trainer = Trainer.Create(command.Name , validEmail , command.Specialization , validPrice );

            _trainerRepository.Add(trainer);

            await _uow.SaveChangesAsync(cancellationToken);

            return trainer.Id;
        }
    }



}

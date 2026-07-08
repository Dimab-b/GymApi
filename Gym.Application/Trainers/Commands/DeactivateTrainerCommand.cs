using Gym.Domain.Common;
using Gym.Domain.Trainers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Trainers.Commands
{
    public record DeactivateTrainerCommand(Guid Id) : IRequest;

    public class DeactivateTrainerCommandHandler : IRequestHandler<DeactivateTrainerCommand>
    {
        private readonly ITrainerRepository _trainerRepository;
        private readonly IUnitOfWork _uow;

        public DeactivateTrainerCommandHandler(ITrainerRepository trainerRepository , IUnitOfWork uow)
        {
            _trainerRepository = trainerRepository;
            _uow = uow;
        }

        public async Task Handle(DeactivateTrainerCommand command , CancellationToken cancellationToken = default)
        {
            var trainer = await _trainerRepository.GetByIdAsync(command.Id , cancellationToken) ?? throw new KeyNotFoundException("Trainer not found");

            trainer.Deactivate();

            await _uow.SaveChangesAsync(cancellationToken);
        }
    }
}

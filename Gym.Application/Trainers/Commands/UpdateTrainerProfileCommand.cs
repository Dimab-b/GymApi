using Gym.Domain.Common;
using Gym.Domain.Common.VO;
using Gym.Domain.Trainers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Trainers.Commands
{
    public record UpdateTrainerProfileCommand(Guid Id , string Name, string Specialization) : IRequest;


    public class UpdateTrainerProfileCommandHandler : IRequestHandler<UpdateTrainerProfileCommand>
    {
        private readonly ITrainerRepository _trainerRepository;
        private readonly IUnitOfWork _uow;

        public UpdateTrainerProfileCommandHandler(ITrainerRepository trainerRepository , IUnitOfWork uow)
        {
            _trainerRepository = trainerRepository;
            _uow = uow;
        }

        public async Task Handle(UpdateTrainerProfileCommand command , CancellationToken cancellationToken = default)
        {
            var trainer = await _trainerRepository.GetByIdAsync(command.Id, cancellationToken) ?? throw new KeyNotFoundException("Trainer not found");

            trainer.UpdateProfile(command.Name , command.Specialization);

            await _uow.SaveChangesAsync(cancellationToken);
        }
    }
    


}

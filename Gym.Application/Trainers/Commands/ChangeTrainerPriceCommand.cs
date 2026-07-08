using Gym.Domain.Common;
using Gym.Domain.Common.VO;
using Gym.Domain.Trainers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Trainers.Commands
{
    public record ChangeTrainerPriceCommand(Guid Id , decimal NewPrice, string Currency) : IRequest;


    public class ChangeTrainerPriceCommandHandler : IRequestHandler<ChangeTrainerPriceCommand>
    {
        private readonly ITrainerRepository _trainerRepository;
        private readonly IUnitOfWork _uow;

        public ChangeTrainerPriceCommandHandler(ITrainerRepository trainerRepository , IUnitOfWork uow)
        {
            _trainerRepository = trainerRepository;
            _uow = uow;
        }

        public async Task Handle(ChangeTrainerPriceCommand command , CancellationToken cancellationToken = default)
        {
            var validPrice = new Price(command.NewPrice , command.Currency);

            var trainer = await _trainerRepository.GetByIdAsync(command.Id , cancellationToken);

            if (trainer == null)
            {
                throw new KeyNotFoundException("That trainer not found");
            }

            trainer.ChangePrice(validPrice);

            await _uow.SaveChangesAsync(cancellationToken);
        }
    }
}

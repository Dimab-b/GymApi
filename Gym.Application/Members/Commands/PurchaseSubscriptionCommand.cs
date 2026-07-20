using Gym.Domain.Common;
using Gym.Domain.Common.VO;
using Gym.Domain.Members;
using Gym.Domain.Members.Value_Objects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Members.Commands
{
    public record PurchaseSubscriptionCommand
        (
        Guid Id,
        int DurationMonths,
        decimal Value,
        string Currency
        ) : IRequest;

    public class PurchaseSubscriptionCommandHandler : IRequestHandler<PurchaseSubscriptionCommand>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMemberRepository _memberRepository;

        public PurchaseSubscriptionCommandHandler(IUnitOfWork uow , IMemberRepository memberRepository)
        {
            _uow = uow;
            _memberRepository = memberRepository;
        }

        public async Task Handle(PurchaseSubscriptionCommand command , CancellationToken cancellationToken = default)
        {
            var member = await _memberRepository.GetByIdAsync( command.Id , cancellationToken );

            if (member == null)
                throw new KeyNotFoundException("That member is not founded");

            member.PurchaseSubscription(command.DurationMonths, new Price(command.Value, command.Currency));

            await _uow.SaveChangesAsync(cancellationToken);
        }
    }



}

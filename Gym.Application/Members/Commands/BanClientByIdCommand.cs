using Gym.Domain.Common;
using Gym.Domain.Members;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Members.Commands
{
    public record BanClientByIdCommand(Guid Id) : IRequest<bool>;
    
    public class BanClientByIdCommandHandler : IRequestHandler<BanClientByIdCommand , bool>
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IUnitOfWork _uow;

        public BanClientByIdCommandHandler(IMemberRepository memberRepository , IUnitOfWork uow)
        {
            _memberRepository = memberRepository;
            _uow = uow;
        }

        public async Task<bool> Handle(BanClientByIdCommand command , CancellationToken cancellationToken = default)
        {
            var member = await _memberRepository.GetByIdAsync(command.Id);

            if (member == null)
            {
                throw new KeyNotFoundException("That member not found");
            }


            member.BanUser();

            await _uow.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

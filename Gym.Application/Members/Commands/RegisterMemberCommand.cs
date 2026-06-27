using Gym.Domain.Common;
using Gym.Domain.Members;
using Gym.Domain.Members.Value_Objects;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Gym.Application.Members.Commands
{
    public record RegisterMemberCommand
        (
        string Name ,
        string Email
        ) : IRequest<Guid>;
    public class RegisterMemberCommandHandler : IRequestHandler<RegisterMemberCommand , Guid>
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IUnitOfWork _uow;

        public RegisterMemberCommandHandler(IMemberRepository memberRepository , IUnitOfWork uow)
        {
            _memberRepository = memberRepository;
            _uow = uow;
        }
        public async Task<Guid> Handle(RegisterMemberCommand command , CancellationToken cancellationToken = default)
        {
            var validEmail = new Email(command.Email);

            var emailExists = await _memberRepository.ExistsByEmailAsync(validEmail , cancellationToken);
            if (emailExists)
                throw new InvalidOperationException("Member with this email already exists.");

            var newMember = Member.Create(command.Name, validEmail);

            _memberRepository.Add(newMember);
            await _uow.SaveChangesAsync(cancellationToken);

            return newMember.Id;
        }
    }
}

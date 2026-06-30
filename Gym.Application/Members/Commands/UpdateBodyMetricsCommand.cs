using Gym.Domain.Common;
using Gym.Domain.Members;
using Gym.Domain.Members.Value_Objects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Members.Commands
{
    public record UpdateBodyMetricsCommand(Guid Id, decimal height, decimal weight, int age, string goal) : IRequest;


    public class UpdateBodyMetricsCommandHandler : IRequestHandler<UpdateBodyMetricsCommand>
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IUnitOfWork _uow;

        public UpdateBodyMetricsCommandHandler(IMemberRepository memberRepository , IUnitOfWork uow)
        {
            _memberRepository = memberRepository;
            _uow = uow;
        }

        public async Task Handle(UpdateBodyMetricsCommand command, CancellationToken cancellationToken = default)
        {
            var member = await _memberRepository.GetByIdAsync(command.Id , cancellationToken);

            if (member == null)
            {
                throw new KeyNotFoundException("That member not found");
            }

            

            member.UpdateBodyMetrics(command.height, command.weight, command.age, command.goal);



            await _uow.SaveChangesAsync(cancellationToken);


        }

    }



}

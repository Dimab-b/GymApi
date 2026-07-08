using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Trainers.Commands
{
    public record CreateTrainerCommand(string Name , string Email , string Specialization , decimal SessionPrice , string Currency) : IRequest<Guid>;


    public class CreateTrainerCommandHandler : IRequestHandler<CreateTrainerCommand , Guid>
    {

    }



}

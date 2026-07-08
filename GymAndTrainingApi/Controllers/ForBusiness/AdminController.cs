using Gym.Application.Members.Commands;
using Gym.Application.Members.Dto_s;
using Gym.Application.Members.Queries;
using Gym.Application.Trainers.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gym.Api.Controllers.ForBusiness
{

    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("members/get-all-members-with-last-subscription")]
        public async Task<IEnumerable<MemberReadDto>> GetAllMembers(CancellationToken cancellationToken = default)
        {
            return await _mediator.Send(new GetAllMembersWithLatestSubscriptionQuery() , cancellationToken);
        }


        [HttpPatch("members/{id:guid}/ban-user")]
        public async Task<ActionResult<bool>> BanUserById(Guid id , CancellationToken cancellationToken = default)
        {
            var command = new BanClientByIdCommand(id);

            var res = await _mediator.Send(command , cancellationToken);

            return Ok(res);
        }


        [HttpGet("members/{id:guid}")]
        public async Task<ActionResult<MemberReadDto>> GetMemberById(Guid id , CancellationToken cancellationToken = default)
        {
            var request = new GetMemberByIdQuery(id);

            var res = await _mediator.Send(request , cancellationToken);

            return Ok(res);
        }

        [HttpGet("stats")]
        public async Task<ActionResult<AdminStatsDto>> GetStats(CancellationToken cancellationToken = default)
        {
            var res = await _mediator.Send(new GetAdminStatsQuery() , cancellationToken);

            return Ok(res);
        }

        [HttpPost("trainers")]
        public async Task<ActionResult<Guid>> CreateTrainer(CreateTrainerCommand command , CancellationToken cancellationToken = default)
        {
            var trainerId = await _mediator.Send(command);
            return CreatedAtAction(nameof(CreateTrainer), new { id = trainerId }, trainerId);
        }

        [HttpPatch("trainers/{id:guid}/price")]
        public async Task<IActionResult> ChangeTrainerPrice(Guid id , NewTrainerPrice request , CancellationToken cancellationToken = default)
        {
            var command = new ChangeTrainerPriceCommand(id , request.NewPrice , request.Currency);

            await _mediator.Send(command , cancellationToken);

            return Ok();
        }

        [HttpPatch("trainers/{id:guid}/deactivate")]
        public async Task<IActionResult> DeactivateTrainer(Guid id)
        {
            var command = new DeactivateTrainerCommand(id);
            await _mediator.Send(command);

            return Ok();
        }


        public record NewTrainerPrice(decimal NewPrice, string Currency);
    }
}

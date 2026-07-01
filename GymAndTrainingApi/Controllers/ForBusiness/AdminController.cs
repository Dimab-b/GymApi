using Gym.Application.Members.Commands;
using Gym.Application.Members.Dto_s;
using Gym.Application.Members.Queries;
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
    }
}

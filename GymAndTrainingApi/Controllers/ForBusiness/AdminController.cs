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
        public async Task<IEnumerable<MemberReadDto>> GetAllMembers()
        {
            return await _mediator.Send(new GetAllMembersWithLatestSubscriptionQuery());
        }


        [HttpPatch("members/{id:guid}/ban-user")]
        public async Task<ActionResult<bool>> BanUserById(Guid id)
        {
            var command = new BanClientByIdCommand(id);

            var res = await _mediator.Send(command);

            return Ok(res);
        }


        [HttpGet("members/{id:guid}")]
        public async Task<ActionResult<MemberReadDto>> GetMemberById(Guid id)
        {
            var request = new GetMemberByIdQuery(id);

            var res = await _mediator.Send(request);

            return Ok(res);
        }
    }
}

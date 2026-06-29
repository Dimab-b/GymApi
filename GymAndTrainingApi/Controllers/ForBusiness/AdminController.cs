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
    }
}

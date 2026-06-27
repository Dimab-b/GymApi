using Gym.Application.Members.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;

namespace Gym.Api.Controllers.ForClient
{
    [ApiController]
    [Route("[controller]")]
    public class MemberController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MemberController(IMediator mediator) { _mediator = mediator; }



        [HttpPost("Register")]
        public async Task<ActionResult<Guid>> RegisterMember(RegisterMemberCommand command , CancellationToken cancellationToken = default)
        {
            var memberId = await _mediator.Send(command , cancellationToken);

            return CreatedAtAction(nameof(RegisterMember), new { id = memberId }, memberId);
        }


        [HttpPost("{id:guid}/subscriptions")]
        public async Task<IActionResult> PurchaseSubscription(Guid id, [FromBody] PurchaseSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var command = new PurchaseSubscriptionCommand(id, request.DurationMonths, request.Value, request.Currency);

            await _mediator.Send(command, cancellationToken);

            return Ok("Subscription successfully purchased.");
        }
    }
}

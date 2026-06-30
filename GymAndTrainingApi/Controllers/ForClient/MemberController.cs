using Gym.Application.Members.Commands;
using Gym.Application.Members.Dto_s;
using Gym.Application.Members.Queries;
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



        [HttpPost("/members/register")]
        public async Task<ActionResult<Guid>> RegisterMember(RegisterMemberCommand command , CancellationToken cancellationToken = default)
        {
            var memberId = await _mediator.Send(command , cancellationToken);

            return CreatedAtAction(nameof(RegisterMember), new { id = memberId }, memberId);
        }


        [HttpPost("{id:guid}/subscriptions/purchase")]
        public async Task<IActionResult> PurchaseSubscription(Guid id, [FromBody] PurchaseSubscriptionRequest request, CancellationToken cancellationToken)
        {
            var command = new PurchaseSubscriptionCommand(id, request.DurationMonths, request.Value, request.Currency);

            await _mediator.Send(command, cancellationToken);

            return Ok("Subscription successfully purchased.");
        }



        [HttpPost("{id:guid}/subscriptions/extend")]
        public async Task<IActionResult> ExtendSubscription(Guid id , [FromBody] ExtendSubscriptionRequest request , CancellationToken cancellationToken = default )
        {
            var command = new ExtendSubscriptionCommand(id , request.ExtraMonths);

            await _mediator.Send(command, cancellationToken);

            return Ok("Subscription successfully purchased.");
        }


        [HttpPatch("/members/{id:guid}/update-body-metrics")]
        public async Task<IActionResult> UpdateBodyMetrics(Guid id, [FromBody] UpdateBodyMetricsRequest request , CancellationToken cancellationToken = default)
        {
            var command = new UpdateBodyMetricsCommand(id ,request.height , request.weight , request.age , request.goal);

            await _mediator.Send(command, cancellationToken);

            return Ok("Body Metrics successfully updated");
        }


        public record PurchaseSubscriptionRequest(int DurationMonths , decimal Value , string Currency);
        public record ExtendSubscriptionRequest(int ExtraMonths);
        public record UpdateBodyMetricsRequest(decimal height, decimal weight, int age, string goal);
    }
}

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



        [HttpPost("register")]
        public async Task<ActionResult<Guid>> RegisterMember(RegisterMemberCommand command , CancellationToken cancellationToken = default)
        {
            var memberId = await _mediator.Send(command , cancellationToken);

            return CreatedAtAction(nameof(RegisterMember), new { id = memberId }, memberId);
        }


        [HttpPost("{id:guid}/profile/subscriptions/purchase")]
        public async Task<IActionResult> PurchaseSubscription(Guid id, [FromBody] PurchaseSubscriptionRequest request, CancellationToken cancellationToken)
        {
            var command = new PurchaseSubscriptionCommand(id, request.DurationMonths, request.Value, request.Currency);

            await _mediator.Send(command, cancellationToken);

            return Ok("Subscription successfully purchased.");
        }



        [HttpPost("{id:guid}/profile/subscriptions/extend")]
        public async Task<IActionResult> ExtendSubscription(Guid id , [FromBody] ExtendSubscriptionRequest request , CancellationToken cancellationToken = default )
        {
            var command = new ExtendSubscriptionCommand(id , request.ExtraMonths);

            await _mediator.Send(command, cancellationToken);

            return Ok("Subscription successfully extended.");
        }


        [HttpPatch("{id:guid}/profile/body-metrics")]
        public async Task<IActionResult> UpdateBodyMetrics(Guid id, [FromBody] UpdateBodyMetricsRequest request , CancellationToken cancellationToken = default)
        {
            var command = new UpdateBodyMetricsCommand(id ,request.Height , request.Weight , request.Age , request.Goal);

            await _mediator.Send(command, cancellationToken);

            return Ok("Body Metrics successfully updated");
        }

        [HttpGet("{id:guid}/profile")]
        public async Task<ActionResult<MemberReadDto>> GetMember(Guid id , CancellationToken cancellationToken = default)
        {
            var query = new GetMemberByIdWithSubscriptionsQuery(id);

            var res = await _mediator.Send(query, cancellationToken);

            return Ok(res);
        }


        public record PurchaseSubscriptionRequest(int DurationMonths , decimal Value , string Currency);
        public record ExtendSubscriptionRequest(int ExtraMonths);
        public record UpdateBodyMetricsRequest(decimal Height, decimal Weight, int Age, string Goal);
    }
}

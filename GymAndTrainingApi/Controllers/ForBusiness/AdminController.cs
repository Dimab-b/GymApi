using Gym.Application.Bookings.Commands;
using Gym.Application.Bookings.Dto_s;
using Gym.Application.Bookings.Queries;
using Gym.Application.Common;
using Gym.Application.Common.Exceptions;
using Gym.Application.Members.Commands;
using Gym.Application.Members.Dto_s;
using Gym.Application.Members.Queries;
using Gym.Application.Payments.Dto_s;
using Gym.Application.Payments.Queries;
using Gym.Application.Trainers.Commands;
using Gym.Application.Trainers.Dto_s;
using Gym.Application.Trainers.Queries;
using Gym.Domain.Payments;
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
            return await _mediator.Send(new GetAllMembersWithLatestSubscriptionQuery(), cancellationToken);
        }


        [HttpPatch("members/{id:guid}/ban-user")]
        public async Task<ActionResult<bool>> BanUserById(Guid id, CancellationToken cancellationToken = default)
        {
            var command = new BanClientByIdCommand(id);

            var res = await _mediator.Send(command, cancellationToken);

            return Ok(res);
        }


        [HttpGet("members/{id:guid}")]
        public async Task<ActionResult<MemberReadDto>> GetMemberById(Guid id, CancellationToken cancellationToken = default)
        {
            var request = new GetMemberByIdQuery(id);

            var res = await _mediator.Send(request, cancellationToken);

            return Ok(res);
        }

        [HttpPost("members/{id:guid}/subscriptions/purchase")]
        public async Task<ActionResult<PurchaseResultDto>> PurchaseSubscription(
            Guid id,
            [FromBody] CashDeskPurchaseRequest request,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey = null,
            CancellationToken cancellationToken = default)
        {
            var method = ResolveCashDeskMethod(request.Method);
            var key = ResolveIdempotencyKey(idempotencyKey);

            var command = new PurchaseSubscriptionCommand(
                id,
                request.PlanCode,
                request.DurationMonths,
                method,
                PaymentPurpose.NewSubscription,
                key);

            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }

        [HttpPost("members/{id:guid}/subscriptions/extend")]
        public async Task<ActionResult<PurchaseResultDto>> ExtendSubscription(
            Guid id,
            [FromBody] CashDeskPurchaseRequest request,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey = null,
            CancellationToken cancellationToken = default)
        {
            var method = ResolveCashDeskMethod(request.Method);
            var key = ResolveIdempotencyKey(idempotencyKey);

            var command = new PurchaseSubscriptionCommand(
                id,
                request.PlanCode,
                request.DurationMonths,
                method,
                PaymentPurpose.Extension,
                key);

            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }

        [HttpGet("members/{id:guid}/payments")]
        public async Task<ActionResult<PagedResult<PaymentDto>>> GetMemberPayments(
            Guid id,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var query = new GetMemberPaymentsQuery(id, pageNumber, pageSize);

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("revenue")]
        public async Task<ActionResult<RevenueDto>> GetRevenue(
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to,
            [FromQuery] PaymentMethod? method,
            [FromQuery] PaymentPurpose? purpose,
            CancellationToken cancellationToken = default)
        {
            var query = new GetRevenueQuery(from, to, method, purpose);

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpPost("trainers")]
        public async Task<ActionResult<Guid>> CreateTrainer(CreateTrainerCommand command, CancellationToken cancellationToken = default)
        {
            var trainerId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetTrainerById), new { id = trainerId }, trainerId);
        }

        [HttpPatch("trainers/{id:guid}/price")]
        public async Task<IActionResult> ChangeTrainerPrice(Guid id, NewTrainerPriceRequest request, CancellationToken cancellationToken = default)
        {
            var command = new ChangeTrainerPriceCommand(id, request.NewPrice, request.Currency);

            await _mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpPatch("trainers/{id:guid}/deactivate")]
        public async Task<IActionResult> DeactivateTrainer(Guid id, CancellationToken cancellationToken = default)
        {
            var command = new DeactivateTrainerCommand(id);
            await _mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpPut("trainers/{id:guid}")]
        public async Task<IActionResult> UpdateProfile(Guid id , UpdateTrainerProfileRequest request , CancellationToken cancellationToken = default)
        {
            var command = new UpdateTrainerProfileCommand(id , request.Name , request.Specialization);

            await _mediator.Send(command ,cancellationToken);

            return NoContent();
        }

        [HttpPatch("bookings/{bookingId:guid}/complete")]
        public async Task<IActionResult> CompleteBooking(Guid bookingId, CancellationToken cancellationToken = default)
        {
            var command = new CompleteBookingCommand(bookingId);

            await _mediator.Send(command, cancellationToken);

            return NoContent();
        }

        [HttpGet("bookings")]
        public async Task<ActionResult<PagedResult<BookingAdminDto>>> GetBookings([FromQuery] GetBookingsAdminQuery query , CancellationToken cancellationToken = default)
        {
            var bookings = await _mediator.Send(query , cancellationToken);

            return Ok(bookings);
        }

        [HttpGet("trainers")]
        public async Task<ActionResult<PagedResult<TrainerReadDto>>> GetTrainers([FromQuery] GetTrainersAdminQuery query , CancellationToken cancellationToken = default)
        {
            var trainers = await _mediator.Send(query , cancellationToken);

            return Ok(trainers);
        }

        [HttpGet("trainers/{id:guid}")]
        public async Task<ActionResult<TrainerAdminDetailsDto>> GetTrainerById(Guid id , CancellationToken cancellationToken = default)
        {
            var query = new GetTrainerByIdAdminQuery(id);

            var trainer = await _mediator.Send(query ,cancellationToken);

            return Ok(trainer);
        }


        private static PaymentMethod ResolveCashDeskMethod(string? method)
        {
            if (!Enum.TryParse<PaymentMethod>(method, ignoreCase: true, out var parsed) ||
                parsed is not (PaymentMethod.Cash or PaymentMethod.Terminal))
                throw new ArgumentException("Cash-desk operations accept only Cash or Terminal.", nameof(method));

            return parsed;
        }

        private static Guid ResolveIdempotencyKey(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new IdempotencyKeyRequiredException();

            if (!Guid.TryParse(value, out var idempotencyKey) || idempotencyKey == Guid.Empty)
                throw new ArgumentException("Idempotency-Key must be a non-empty GUID.", nameof(value));

            return idempotencyKey;
        }

        public record CashDeskPurchaseRequest(string PlanCode, int DurationMonths, string? Method);
        public record NewTrainerPriceRequest(decimal NewPrice, string Currency);
        public record UpdateTrainerProfileRequest(string Name, string Specialization);
    }
}

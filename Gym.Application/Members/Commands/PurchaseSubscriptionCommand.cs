using Gym.Application.Common.Interfaces;
using Gym.Application.Members.Dto_s;
using Gym.Domain.Common;
using Gym.Domain.Members;
using Gym.Domain.Payments;
using MediatR;

namespace Gym.Application.Members.Commands
{
    public record PurchaseSubscriptionCommand(
        Guid MemberId,
        string PlanCode,
        int DurationMonths,
        PaymentMethod Method,
        PaymentPurpose Purpose) : IRequest<PurchaseResultDto>;

    public class PurchaseSubscriptionCommandHandler : IRequestHandler<PurchaseSubscriptionCommand, PurchaseResultDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMemberRepository _memberRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ISubscriptionPlanProvider _planProvider;

        public PurchaseSubscriptionCommandHandler(
            IUnitOfWork uow,
            IMemberRepository memberRepository,
            IPaymentRepository paymentRepository,
            ISubscriptionPlanProvider planProvider)
        {
            _uow = uow;
            _memberRepository = memberRepository;
            _paymentRepository = paymentRepository;
            _planProvider = planProvider;
        }

        public async Task<PurchaseResultDto> Handle(
            PurchaseSubscriptionCommand command,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(command.PlanCode))
                throw new ArgumentException("Plan code must be filled.", nameof(command.PlanCode));

            if (!Enum.IsDefined(command.Method))
                throw new ArgumentException("Payment method is invalid.", nameof(command.Method));

            if (!Enum.IsDefined(command.Purpose))
                throw new ArgumentException("Payment purpose is invalid.", nameof(command.Purpose));

            var plan = _planProvider.GetByCode(command.PlanCode)
                ?? throw new InvalidOperationException($"Plan '{command.PlanCode}' is not available.");

            return await _uow.ExecuteInTransactionAsync(async ct =>
            {
                await _memberRepository.LockForPurchaseAsync(command.MemberId, ct);

                var member = await _memberRepository.GetByIdAsync(command.MemberId, ct)
                    ?? throw new KeyNotFoundException($"Member with ID {command.MemberId} was not found.");

                var subscription = member.PurchaseSubscription(
                    plan.Code,
                    plan.PricePerMonth,
                    plan.Currency,
                    command.DurationMonths,
                    plan.VisitLimitPerMonth);

                var payment = Payment.Create(
                    member.Id,
                    subscription.Id,
                    subscription.Price,
                    command.Method,
                    command.Purpose);

                _paymentRepository.Add(payment);

                await _uow.SaveChangesAsync(ct);

                return new PurchaseResultDto(
                    subscription.Id,
                    subscription.Price.Value,
                    subscription.Price.Currency);
            }, cancellationToken);
        }
    }
}

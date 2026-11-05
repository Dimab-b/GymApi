using System.Security.Cryptography;
using System.Text;
using Gym.Application.Common.Exceptions;
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
        PaymentPurpose Purpose,
        Guid? IdempotencyKey = null) : IRequest<PurchaseResultDto>;

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
            var plan = _planProvider.GetByCode(command.PlanCode)
                ?? throw new PlanUnavailableException(command.PlanCode);

            var requestHash = command.IdempotencyKey.HasValue
                ? ComputeRequestHash(
                    command.MemberId,
                    plan.Code,
                    command.DurationMonths,
                    command.Method,
                    command.Purpose)
                : null;

            if (command.IdempotencyKey is { } idempotencyKey)
            {
                var existing = await _paymentRepository.GetByIdempotencyKeyAsync(
                    command.MemberId,
                    idempotencyKey,
                    cancellationToken);

                if (existing is not null)
                    return Replay(existing, requestHash);
            }

            try
            {
                return await _uow.ExecuteInTransactionAsync(async ct =>
                {
                    await _memberRepository.LockForPurchaseAsync(command.MemberId, ct);

                    var member = await _memberRepository.GetByIdAsync(command.MemberId, ct)
                        ?? throw new MemberNotFoundException(command.MemberId);

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
                        command.Purpose,
                        note: null,
                        idempotencyKey: command.IdempotencyKey,
                        idempotencyRequestHash: requestHash);

                    _paymentRepository.Add(payment);

                    await _uow.SaveChangesAsync(ct);

                    return new PurchaseResultDto(
                        subscription.Id,
                        subscription.Price.Value,
                        subscription.Price.Currency);
                }, cancellationToken);
            }
            catch (DataConflictException) when (command.IdempotencyKey.HasValue)
            {
                var existing = await _paymentRepository.GetByIdempotencyKeyAsync(
                    command.MemberId,
                    command.IdempotencyKey.Value,
                    cancellationToken);

                if (existing is not null)
                    return Replay(existing, requestHash);

                throw;
            }
            catch (SubscriptionConflictException) when (command.IdempotencyKey.HasValue)
            {
                var existing = await WaitForExistingPaymentAsync(
                    command.MemberId,
                    command.IdempotencyKey.Value,
                    cancellationToken);

                if (existing is not null)
                    return Replay(existing, requestHash);

                throw;
            }
        }

        private async Task<Payment?> WaitForExistingPaymentAsync(
            Guid memberId,
            Guid idempotencyKey,
            CancellationToken cancellationToken)
        {
            for (var attempt = 0; attempt < 10; attempt++)
            {
                var existing = await _paymentRepository.GetByIdempotencyKeyAsync(
                    memberId,
                    idempotencyKey,
                    cancellationToken);

                if (existing is not null)
                    return existing;

                await Task.Delay(50, cancellationToken);
            }

            return null;
        }

        private static PurchaseResultDto Replay(Payment payment, string? requestHash)
        {
            if (!string.Equals(payment.IdempotencyRequestHash, requestHash, StringComparison.Ordinal))
                throw new IdempotencyConflictException();

            if (payment.SubscriptionId is not { } subscriptionId)
                throw new InvalidOperationException("The stored payment is not linked to a subscription.");

            return new PurchaseResultDto(
                subscriptionId,
                payment.Amount.Value,
                payment.Amount.Currency);
        }

        private static string ComputeRequestHash(
            Guid memberId,
            string planCode,
            int durationMonths,
            PaymentMethod method,
            PaymentPurpose purpose)
        {
            var canonical = string.Join(
                '|',
                memberId.ToString("N"),
                planCode.Trim().ToLowerInvariant(),
                durationMonths,
                (int)method,
                (int)purpose);

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}

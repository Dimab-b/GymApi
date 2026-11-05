using Dapper;
using Gym.Application.Common;
using Gym.Application.Payments.Dto_s;
using MediatR;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Gym.Application.Payments.Queries
{
    public record GetMemberPaymentsQuery(
        Guid MemberId,
        int PageNumber = 1,
        int PageSize = 20) : IRequest<PagedResult<PaymentDto>>;

    public class GetMemberPaymentsQueryHandler : IRequestHandler<GetMemberPaymentsQuery, PagedResult<PaymentDto>>
    {
        private readonly string _connection;

        public GetMemberPaymentsQueryHandler(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string is missing.");
        }

        public async Task<PagedResult<PaymentDto>> Handle(
            GetMemberPaymentsQuery request,
            CancellationToken cancellationToken = default)
        {
            if (request.PageNumber < 1 || request.PageNumber > 100000)
                throw new ArgumentException("Page number must be between 1 and 100000.", nameof(request.PageNumber));

            if (request.PageSize < 1 || request.PageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100.", nameof(request.PageSize));

            await using var connection = new NpgsqlConnection(_connection);

            const string sql = @"
                SELECT COUNT(*)::int FROM ""Payments"" WHERE ""MemberId"" = @MemberId;

                SELECT
                    ""Id"",
                    ""MemberId"",
                    ""SubscriptionId"",
                    ""AmountValue"" AS ""Amount"",
                    ""AmountCurrency"" AS ""Currency"",
                    ""PaidAt"",
                    ""Method"",
                    ""Purpose"",
                    ""Status"",
                    ""Note""
                FROM ""Payments""
                WHERE ""MemberId"" = @MemberId
                ORDER BY ""PaidAt"" DESC, ""Id"" DESC
                LIMIT @PageSize OFFSET @Offset;";

            var command = new CommandDefinition(
                sql,
                new
                {
                    request.MemberId,
                    request.PageSize,
                    Offset = (long)(request.PageNumber - 1) * request.PageSize
                },
                cancellationToken: cancellationToken);

            using var multi = await connection.QueryMultipleAsync(command);

            var totalCount = await multi.ReadSingleAsync<int>();
            var payments = await multi.ReadAsync<PaymentDto>();

            return new PagedResult<PaymentDto>(
                payments.ToList(),
                totalCount,
                request.PageNumber,
                request.PageSize);
        }
    }
}

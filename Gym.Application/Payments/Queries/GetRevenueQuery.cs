using Dapper;
using Gym.Application.Payments.Dto_s;
using Gym.Domain.Payments;
using MediatR;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Gym.Application.Payments.Queries
{
    public record GetRevenueQuery(
        DateOnly From,
        DateOnly To,
        PaymentMethod? Method = null,
        PaymentPurpose? Purpose = null) : IRequest<RevenueDto>;

    public class GetRevenueQueryHandler : IRequestHandler<GetRevenueQuery, RevenueDto>
    {
        private readonly string _connection;

        public GetRevenueQueryHandler(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string is missing.");
        }

        public async Task<RevenueDto> Handle(
            GetRevenueQuery request,
            CancellationToken cancellationToken = default)
        {
            if (request.From > request.To)
                throw new ArgumentException("The start date must be earlier than or equal to the end date.");

            if (request.To == DateOnly.MaxValue)
                throw new ArgumentException("The end date is out of the supported range.", nameof(request.To));

            var fromUtc = request.From.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var toExclusiveUtc = request.To.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

            await using var connection = new NpgsqlConnection(_connection);

            const string sql = @"
                SELECT
                    ""AmountCurrency"" AS ""Currency"",
                    SUM(""AmountValue"") AS ""Amount""
                FROM ""Payments""
                WHERE ""PaidAt"" >= @FromUtc
                  AND ""PaidAt"" < @ToExclusiveUtc
                  AND (@Method::text IS NULL OR ""Method"" = @Method)
                  AND (@Purpose::text IS NULL OR ""Purpose"" = @Purpose)
                GROUP BY ""AmountCurrency""
                ORDER BY ""AmountCurrency"";";

            var command = new CommandDefinition(
                sql,
                new
                {
                    FromUtc = fromUtc,
                    ToExclusiveUtc = toExclusiveUtc,
                    Method = request.Method?.ToString(),
                    Purpose = request.Purpose?.ToString()
                },
                cancellationToken: cancellationToken);

            var totals = await connection.QueryAsync<RevenueTotalDto>(command);

            return new RevenueDto(request.From, request.To, totals.ToList());
        }
    }
}

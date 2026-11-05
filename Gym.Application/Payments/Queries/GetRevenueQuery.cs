using Dapper;
using Gym.Application.Common.Time;
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
        private readonly GymTimeZone _timeZone;

        public GetRevenueQueryHandler(IConfiguration configuration, GymTimeZone timeZone)
        {
            _connection = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string is missing.");
            _timeZone = timeZone;
        }

        public async Task<RevenueDto> Handle(
            GetRevenueQuery request,
            CancellationToken cancellationToken = default)
        {
            var fromUtc = _timeZone.ToUtc(request.From);
            var toExclusiveUtc = _timeZone.ToUtc(request.To.AddDays(1));

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

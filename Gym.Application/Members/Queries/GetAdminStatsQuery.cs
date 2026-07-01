using Dapper;
using Gym.Application.Members.Dto_s;
using MediatR;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Members.Queries
{
    public record GetAdminStatsQuery() : IRequest<AdminStatsDto>;

    public class GetAdminStatsQueryHandler : IRequestHandler<GetAdminStatsQuery , AdminStatsDto>
    {
        private readonly string _connection;
        
        public GetAdminStatsQueryHandler(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is missing.");
        }

        public async Task<AdminStatsDto> Handle(GetAdminStatsQuery query , CancellationToken cancellationToken = default)
        {
            await using var connection = new NpgsqlConnection(_connection);

            var sql = @"
            SELECT 
            COUNT(DISTINCT ""MemberId"") FILTER (WHERE ""EndDate"" >= CURRENT_TIMESTAMP) AS ""CountActiveSubs"",
            COALESCE(SUM(""PriceAmount"") FILTER (WHERE ""StartDate"" >= CURRENT_DATE - INTERVAL '1 month'), 0) AS ""TotalProfitLastMonth""
            FROM ""Subscriptions"" ";

            var command = new CommandDefinition(sql , cancellationToken: cancellationToken);

            var stats = await connection.QuerySingleAsync<AdminStatsDto>(command);

            return stats; 
        }
    }



}

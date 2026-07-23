using Dapper;
using Gym.Application.Common;
using Gym.Application.Trainers.Dto_s;
using MediatR;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Trainers.Queries
{
    public record GetActiveTrainersQuery(string? Specialization , string? OrderBy , int PageNumber, int PageSize) : IRequest<PagedResult<TrainerReadDto>>;


    public class GetActiveTrainersQueryHandler : IRequestHandler<GetActiveTrainersQuery, PagedResult<TrainerReadDto>>
    {
        private readonly string _connection;
        public GetActiveTrainersQueryHandler(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is missing.");
        }

        public async Task<PagedResult<TrainerReadDto>> Handle(GetActiveTrainersQuery query ,CancellationToken cancellationToken = default)
        {
            await using var connection = new NpgsqlConnection(_connection);

            int offset = (query.PageNumber - 1) * query.PageSize;

            var whereConditions = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(query.Specialization))
            {
                whereConditions.Add(@"""Specialization"" ILIKE @Specialization");
                parameters.Add("Specialization", $"%{query.Specialization.Trim()}%");
            }

            string orderByClause = @"ORDER BY ""Name"" ASC";

            if (!string.IsNullOrWhiteSpace(query.OrderBy))
            {
                if (query.OrderBy.Equals("High", StringComparison.OrdinalIgnoreCase))
                {
                    orderByClause = @"ORDER BY ""SessionPrice_Value"" DESC";
                }
                else if (query.OrderBy.Equals("Low", StringComparison.OrdinalIgnoreCase))
                {
                    orderByClause = @"ORDER BY ""SessionPrice_Value"" ASC";
                }
            }

            parameters.Add("PageSize" , query.PageSize);
            parameters.Add("Offset", offset);

            var whereClause = whereConditions.Any()
            ? "AND " + string.Join( " " ,whereConditions)
            : "";
           

            var sql = $@"SELECT 
                    ""Id"",
                    ""Name"", 
                    ""Specialization"", 
                    ""SessionPrice_Value"" AS Price, 
                    ""SessionPrice_Currency"" AS Currency
                    FROM ""Trainers""
                    WHERE ""IsActive"" = true
                    {whereClause}
                    {orderByClause}
                    LIMIT @PageSize OFFSET @Offset;

                    SELECT COUNT(*)
                    FROM ""Trainers"" 
                    WHERE ""IsActive"" = true
                    {whereClause};";

            var command = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken
            );

            using var multi = await connection.QueryMultipleAsync(command);

            var items = (await multi.ReadAsync<TrainerReadDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return new PagedResult<TrainerReadDto>(
                items,
                totalCount,
                query.PageNumber,
                query.PageSize
            );


        }
    }
}

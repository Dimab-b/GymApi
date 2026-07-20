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
    public record GetTrainersAdminQuery(int PageNumber, int PageSize) : IRequest<PagedResult<TrainerReadDto>>;
    
    public class GetTrainersAdminQueryHandler : IRequestHandler<GetTrainersAdminQuery , PagedResult<TrainerReadDto>>
    {
        private readonly string _connection;
        
        public GetTrainersAdminQueryHandler(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is missing.");
        }

        public async Task<PagedResult<TrainerReadDto>> Handle(GetTrainersAdminQuery query , CancellationToken cancellationToken = default)
        {
            await using var connection = new NpgsqlConnection(_connection);

            int offset = (query.PageNumber - 1) * query.PageSize;

            var sql = @"SELECT COUNT(*) FROM ""Trainers"";

                SELECT 
                    ""Id"", 
                    ""Name"", 
                    ""Specialization"", 
                    ""SessionPrice_Value"" AS Price, 
                    ""SessionPrice_Currency"" AS Currency
                FROM ""Trainers""
                ORDER BY ""Name""
                LIMIT @PageSize OFFSET @Offset;";


            var command = new CommandDefinition(sql, new { Offset = offset , PageSize = query.PageSize} , cancellationToken: cancellationToken);

            using var multi = await connection.QueryMultipleAsync(command);

            int totalCount = await multi.ReadFirstAsync<int>();


            var items = (await multi.ReadAsync<TrainerReadDto>()).ToList();


            return new PagedResult<TrainerReadDto>(items, totalCount, query.PageNumber, query.PageSize);
        }
    }
}

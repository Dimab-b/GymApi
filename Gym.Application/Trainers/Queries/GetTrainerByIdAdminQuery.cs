using Dapper;
using Gym.Application.Trainers.Dto_s;
using MediatR;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Trainers.Queries
{
    public record GetTrainerByIdAdminQuery(Guid Id) : IRequest<TrainerAdminDetailsDto>;

    public class GetTrainerByIdAdminQueryHandler : IRequestHandler<GetTrainerByIdAdminQuery , TrainerAdminDetailsDto>
    {
        private readonly string _connection;

        public GetTrainerByIdAdminQueryHandler(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is missing.");
        }

        public async Task<TrainerAdminDetailsDto> Handle(GetTrainerByIdAdminQuery query, CancellationToken cancellationToken = default)
        {
            await using var connection = new NpgsqlConnection(_connection);

            var sql = @"
                SELECT 
                    ""Id"", ""Name"", ""Specialization"", 
                    ""SessionPrice_Value"" AS CurrentPrice, 
                    ""SessionPrice_Currency"" AS Currency
                FROM ""Trainers"" 
                WHERE ""Id"" = @Id;

        
                SELECT 
                    COUNT(*) AS TotalSessionsCompleted,
                    COALESCE(SUM(""Price_Value""), 0) AS TotalEarned,
                    COUNT(DISTINCT ""ClientId"") AS ActiveClientsCount
                FROM ""SessionBookings""
                WHERE ""TrainerId"" = @Id AND ""Status"" = 'Completed';


                SELECT 
                    b.""Id"", 
                    b.""StartTime"", 
                    b.""Status"", 
                    u.""Name"" AS ClientName
                FROM ""SessionBookings"" b
                INNER JOIN ""Users"" u ON b.""ClientId"" = u.""Id""
                WHERE b.""TrainerId"" = @Id
                ORDER BY b.""StartTime"" DESC
                LIMIT 10;";

            var command = new CommandDefinition(sql, new { Id = query.Id }, cancellationToken: cancellationToken);

            using var multi = await connection.QueryMultipleAsync(command);

            var baseInfo = await multi.ReadFirstOrDefaultAsync<dynamic>();

            if (baseInfo == null)
                throw new KeyNotFoundException($"Trainer with Id {query.Id} not found.");

            var stats = await multi.ReadFirstOrDefaultAsync<TrainerStatsDto>()
                        ?? new TrainerStatsDto(0, 0, 0);


            var bookings = (await multi.ReadAsync<TrainerBookingDto>()).ToList();
            
          
            return new TrainerAdminDetailsDto(
                baseInfo.Id,
                baseInfo.Name,
                baseInfo.Specialization,
                baseInfo.currentprice,
                baseInfo.currency,
                stats,
                bookings
            );
        }
    }



}

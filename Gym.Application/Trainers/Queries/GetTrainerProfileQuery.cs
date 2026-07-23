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
    public record GetTrainerProfileQuery(Guid Id) : IRequest<TrainerReadDto>;

    public class GetTrainerProfileQueryHandler : IRequestHandler<GetTrainerProfileQuery , TrainerReadDto>
    {
        private readonly string _connection;

        public GetTrainerProfileQueryHandler(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is missing.");
        }

        public async Task<TrainerReadDto> Handle(GetTrainerProfileQuery query , CancellationToken cancellationToken = default)
        {
            await using var connection = new NpgsqlConnection(_connection);

            var sql = @"SELECT ""Id"", 
                    ""Name"",
                    ""Email_Value"" AS Email,
                    ""Specialization"",
                    ""SessionPrice_Value"" AS Price, 
                    ""SessionPrice_Currency"" AS Currency
            FROM ""Trainers"" WHERE Id = @Id";

            var command = new CommandDefinition(sql, new { Id = query.Id},cancellationToken:cancellationToken);

            var trainer = await connection.QueryFirstOrDefaultAsync<TrainerReadDto>(command);

            if (trainer == null)
                throw new KeyNotFoundException($"Trainer with Id {query.Id} not found.");

            return trainer;
        }
    }



}

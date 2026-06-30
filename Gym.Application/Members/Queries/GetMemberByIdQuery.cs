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
    public record GetMemberByIdQuery(Guid Id) : IRequest<MemberReadDto>;


    public class GetMemberByIdQueryHandler : IRequestHandler<GetMemberByIdQuery , MemberReadDto>
    {
        private readonly string _connection;

        public GetMemberByIdQueryHandler(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is missing.");
        }


        public async Task<MemberReadDto> Handle(GetMemberByIdQuery request , CancellationToken cancellationToken = default)
        {
            await using var connection = new NpgsqlConnection(_connection);

            var sql = @"SELECT * FROM ""Members"" WHERE Id = @Id";

            var command = new CommandDefinition(
            sql,
            new { Id = request.Id },
            cancellationToken: cancellationToken
            );

            var query = await connection.QueryFirstOrDefaultAsync<MemberReadDto>(command);


            if (query == null)
            {
                throw new KeyNotFoundException($"Member with Id {request.Id} not found.");
            }

            return query;
        }
    }
    
    
}

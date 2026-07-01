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
    public record GetMemberByIdWithSubscriptionsQuery(Guid Id) : IRequest<MemberReadDto>;

    public class GetMemberByIdWithSubscriptionsQueryHandler : IRequestHandler<GetMemberByIdWithSubscriptionsQuery , MemberReadDto>
    {
        private readonly string _connection;

        public GetMemberByIdWithSubscriptionsQueryHandler(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is missing.");
        }


        public async Task<MemberReadDto> Handle(GetMemberByIdWithSubscriptionsQuery request , CancellationToken cancellationToken = default)
        {
            await using var connection = new NpgsqlConnection(_connection);

            var sql = @"
        SELECT * FROM ""Members"" WHERE ""Id"" = @Id;
        SELECT * FROM ""Subscriptions"" WHERE ""MemberId"" = @Id;
    ";

            var command = new CommandDefinition(sql, new { Id = request.Id }, cancellationToken: cancellationToken);

            using var multi = await connection.QueryMultipleAsync(command);

            var member = await multi.ReadFirstOrDefaultAsync<MemberReadDto>();

            if (member == null)
            {
                throw new KeyNotFoundException($"Member with ID {request.Id} was not found.");
            }

            var subscriptions = await multi.ReadAsync<SubscriptionReadDto>();

            member.Subscriptions = subscriptions.ToList();

            return member;
        }


    }



}

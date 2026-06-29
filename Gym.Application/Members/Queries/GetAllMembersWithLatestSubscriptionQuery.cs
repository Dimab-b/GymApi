using Gym.Application.Members.Dto_s;
using MediatR;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using System.ComponentModel.Design;

namespace Gym.Application.Members.Queries
{
    public record GetAllMembersWithLatestSubscriptionQuery() : IRequest<IEnumerable<MemberReadDto>>;
    
    public class GetAllMembersWithLatestSubscriptionQueryHandler : IRequestHandler<GetAllMembersWithLatestSubscriptionQuery ,IEnumerable<MemberReadDto>>
    {
        private readonly string _connection;

        public GetAllMembersWithLatestSubscriptionQueryHandler(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is missing.");
        }

        public async Task<IEnumerable<MemberReadDto>> Handle(GetAllMembersWithLatestSubscriptionQuery request , CancellationToken cancellationToken = default)
        { 
            await using var connection = new NpgsqlConnection(_connection);


            var sql = @"SELECT m.*, sub.*
            FROM ""Members"" m
            LEFT JOIN (
            SELECT ""Id"", ""MemberId"", ""StartDate"", ""EndDate"", ""PriceAmount"", ""PriceCurrency"",
            ROW_NUMBER() OVER (PARTITION BY ""MemberId"" ORDER BY ""StartDate"" DESC) as rn
            FROM ""Subscriptions""
            )sub ON m.""Id"" = sub.""MemberId"" AND sub.rn = 1
            ";


            var query = await connection.QueryAsync<MemberReadDto, SubscriptionReadDto, MemberReadDto>(
                sql,
                (member, subscription) =>
                {
         
                    if (subscription != null)
                        {
                            member.Subscriptions.Add(subscription);
                         }
                    return member;
                },
                param: null,      
                splitOn: "Id"

            );

            return query;



            

        }
    }
}

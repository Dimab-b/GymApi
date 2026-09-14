using Dapper;
using Gym.Application.Members.Dto_s;
using MediatR;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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


            var sql = @"
            SELECT ""Id"", ""Name"", ""Email"", ""IsBanned"", ""HeightCm"", ""WeightKg"", ""Age"", ""Goal""
            FROM ""Members""
            ORDER BY ""Name"";

            SELECT DISTINCT ON (s.""MemberId"")
                s.""Id"", s.""MemberId"", s.""StartDate"", s.""EndDate"", s.""PriceAmount"", s.""PriceCurrency"",
                (CURRENT_TIMESTAMP >= s.""StartDate"" AND CURRENT_TIMESTAMP <= s.""EndDate"") AS ""IsActive""
            FROM ""Subscriptions"" s
            ORDER BY s.""MemberId"", s.""StartDate"" DESC;";

            using var multi = await connection.QueryMultipleAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));

            var members = (await multi.ReadAsync<MemberReadDto>()).ToList();
            var latestSubscriptions = (await multi.ReadAsync<SubscriptionRow>()).ToList();

            var subscriptionsByMember = latestSubscriptions
                .GroupBy(s => s.MemberId)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var member in members)
            {
                if (!subscriptionsByMember.TryGetValue(member.Id, out var subscription))
                    continue;

                member.Subscriptions.Add(new SubscriptionReadDto
                {
                    Id = subscription.Id,
                    StartDate = subscription.StartDate,
                    EndDate = subscription.EndDate,
                    PriceAmount = subscription.PriceAmount,
                    PriceCurrency = subscription.PriceCurrency,
                    IsActive = subscription.IsActive
                });
            }

            return members;
        }

        private sealed class SubscriptionRow
        {
            public Guid Id { get; set; }
            public Guid MemberId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public decimal PriceAmount { get; set; }
            public string PriceCurrency { get; set; } = null!;
            public bool IsActive { get; set; }
        }
    }
}

using Dapper;
using Gym.Application.Bookings.Dto_s;
using Gym.Application.Common;
using Gym.Domain.Bookings;
using MediatR;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gym.Application.Bookings.Queries
{
    public record GetBookingsAdminQuery(
        DateTime? From,
        DateTime? To,
        Guid? TrainerId,
        Guid? MemberId,
        BookingStatus? Status,
        int Page,
        int PageSize
    ) : IRequest<PagedResult<BookingAdminDto>>;

    public class GetBookingsAdminQueryHandler : IRequestHandler<GetBookingsAdminQuery, PagedResult<BookingAdminDto>>
    {
        private readonly string _connection;

        public GetBookingsAdminQueryHandler(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is missing.");
        }

        public async Task<PagedResult<BookingAdminDto>> Handle(GetBookingsAdminQuery request, CancellationToken cancellationToken = default)
        {
            await using var connection = new NpgsqlConnection(_connection);

            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize;
            var offset = (page - 1) * pageSize;

            var sql = @"
        SELECT 
            b.""Id"",
            b.""MemberId"",
            m.""Name"" AS ""MemberName"",
            m.""Email"" AS ""MemberEmail"",
            b.""TrainerId"",
            t.""Name"" AS ""TrainerName"",
            t.""Specialization"" AS ""TrainerSpecialization"",
            b.""StartTime"",
            b.""EndTime"",
            b.""Status"",
            b.""FixedPrice_Value"" AS ""PriceAmount"",
            b.""FixedPrice_Currency"" AS ""PriceCurrency""
        FROM ""Bookings"" b
        INNER JOIN ""Trainers"" t ON b.""TrainerId"" = t.""Id""
        INNER JOIN ""Members"" m ON b.""MemberId"" = m.""Id""
        WHERE (@From::timestamptz IS NULL OR b.""StartTime"" >= @From)
          AND (@To::timestamptz IS NULL OR b.""StartTime"" < @To)
          AND (@TrainerId::uuid IS NULL OR b.""TrainerId"" = @TrainerId)
          AND (@MemberId::uuid IS NULL OR b.""MemberId"" = @MemberId)
          AND (@Status::text IS NULL OR b.""Status"" = @Status)
        ORDER BY b.""StartTime"" DESC
        LIMIT @PageSize OFFSET @Offset;

        SELECT COUNT(*)
        FROM ""Bookings"" b
        WHERE (@From::timestamptz IS NULL OR b.""StartTime"" >= @From)
          AND (@To::timestamptz IS NULL OR b.""StartTime"" < @To)
          AND (@TrainerId::uuid IS NULL OR b.""TrainerId"" = @TrainerId)
          AND (@MemberId::uuid IS NULL OR b.""MemberId"" = @MemberId)
          AND (@Status::text IS NULL OR b.""Status"" = @Status);";

            var parameters = new
            {
                request.From,
                request.To,
                request.TrainerId,
                request.MemberId,
                Status = request.Status?.ToString(),
                PageSize = pageSize,
                Offset = offset
            };

            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            using var multi = await connection.QueryMultipleAsync(command);

            var items = (await multi.ReadAsync<BookingAdminDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return new PagedResult<BookingAdminDto>(items, totalCount, page, pageSize);
        }
    }
}

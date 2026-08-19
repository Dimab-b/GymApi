using Dapper;
using Gym.Application.Bookings.Dto_s;
using Gym.Application.Common;
using Gym.Application.Trainers.Dto_s;
using MediatR;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Bookings.Queries
{
    public record GetMyBookingsQuery(Guid MemberId , string? Status ,int PageNumber, int PageSize) : IRequest<PagedResult<BookingReadDto>>;

    public class GetMyBookingsQueryHandler : IRequestHandler<GetMyBookingsQuery , PagedResult<BookingReadDto>>
    {
        private readonly string _connection;
        public GetMyBookingsQueryHandler(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is missing.");
        }

        public async Task<PagedResult<BookingReadDto>> Handle(GetMyBookingsQuery request , CancellationToken cancellationToken = default)
        {
            await using var connection = new NpgsqlConnection(_connection);
            int offset = (request.PageNumber - 1) * request.PageSize;

            string? statusParam = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim();

            var sql = @"
        SELECT 
            b.""Id"",
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
        WHERE b.""MemberId"" = @MemberId
          AND (@Status IS NULL OR b.""Status"" = @Status)
        ORDER BY b.""StartTime"" DESC
        LIMIT @PageSize OFFSET @Offset;

        SELECT COUNT(*) 
        FROM ""Bookings"" b
        WHERE b.""MemberId"" = @MemberId
          AND (@Status IS NULL OR b.""Status"" = @Status);";

            var command = new CommandDefinition(
                sql,
                new
                {
                    MemberId = request.MemberId,
                    Status = statusParam,
                    PageSize = request.PageSize,
                    Offset = offset
                },
                cancellationToken: cancellationToken
            );

            using var multi = await connection.QueryMultipleAsync(command);

            var items = (await multi.ReadAsync<BookingReadDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return new PagedResult<BookingReadDto>(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize
                );
        }
    }



}

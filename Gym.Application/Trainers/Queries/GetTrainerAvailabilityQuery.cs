using Dapper;
using Gym.Application.Common.Time;
using Gym.Application.Trainers.Dto_s;
using Gym.Domain.Bookings;
using MediatR;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Gym.Application.Trainers.Queries
{
    public record GetTrainerAvailabilityQuery(Guid TrainerId , DateOnly Date) : IRequest<List<AvailableSlotDto>>;

    public class GetTrainerAvailabilityQueryHandler : IRequestHandler<GetTrainerAvailabilityQuery ,List<AvailableSlotDto>>
    {
        private readonly string _connection;
        private readonly GymTimeZone _timeZone;

        public GetTrainerAvailabilityQueryHandler(IConfiguration configuration, GymTimeZone timeZone)
        {
            _connection = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is missing.");
            _timeZone = timeZone;
        }

        public async Task<List<AvailableSlotDto>> Handle(GetTrainerAvailabilityQuery query , CancellationToken cancellationToken = default)
        {
            await using var connection = new NpgsqlConnection(_connection);

            var startOfDay = _timeZone.ToUtc(query.Date);
            var endOfDay = _timeZone.ToUtc(query.Date.AddDays(1));

            var sql = @"
            SELECT ""StartTime"", ""EndTime"" 
            FROM ""Bookings"" 
            WHERE ""TrainerId"" = @TrainerId 
              AND ""StartTime"" >= @StartOfDay
              AND ""StartTime"" < @EndOfDay
              AND ""Status"" != @CancelledStatus;";

            var command = new CommandDefinition(sql, new { TrainerId = query.TrainerId , StartOfDay = startOfDay , EndOfDay = endOfDay , CancelledStatus = BookingStatus.Cancelled.ToString() } , cancellationToken: cancellationToken);

            var bookedSlots = (await connection.QueryAsync<BookedSlot>(command)).ToList();

            var workStart = new TimeSpan(9, 0, 0);
            var workEnd = new TimeSpan(18, 0, 0);
            var sessionDuration = TimeSpan.FromHours(1);

            var availableSlots = new List<AvailableSlotDto>();
            var currentSlotStart = workStart;

            var now = DateTime.UtcNow;

            while (currentSlotStart + sessionDuration <= workEnd)
            {
                var currentSlotEnd = currentSlotStart + sessionDuration;
                var currentSlotDateTime = _timeZone.ToUtc(
                    query.Date.ToDateTime(
                        TimeOnly.FromTimeSpan(currentSlotStart),
                        DateTimeKind.Unspecified));

                bool isBooked = bookedSlots.Any(b =>
                    currentSlotStart < _timeZone.ToLocalTimeOfDay(b.EndTime) &&
                    currentSlotEnd > _timeZone.ToLocalTimeOfDay(b.StartTime));
                bool isTooSoon = currentSlotDateTime < now.AddHours(12);

                if (!isBooked && !isTooSoon)
                {
                    availableSlots.Add(new AvailableSlotDto(currentSlotStart, currentSlotEnd));
                }

                currentSlotStart += sessionDuration;
            }

            return availableSlots;
        }
        private class BookedSlot
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }
    }
}

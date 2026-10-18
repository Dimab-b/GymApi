using Gym.Application.Bookings.Commands;
using Hangfire;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.BackgroundJobs
{
    public class BookingJobs
    {
        private readonly IMediator _mediator;

        public BookingJobs(IMediator mediator)
        {
            _mediator = mediator;
        }

        [DisableConcurrentExecution(60)]
        public async Task AutoComplete()
        {
            await _mediator.Send(new CompleteExpiredBookingsCommand());
        }
    }
}

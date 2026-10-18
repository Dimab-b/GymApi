using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.BackgroundJobs
{
    public sealed class RecurringJobsRegistrar : IHostedService
    {
        private const string AutoCompleteJobId = "bookings-autocomplete";

        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IConfiguration _configuration;

        public RecurringJobsRegistrar(IRecurringJobManager recurringJobManager, IConfiguration configuration)
        {
            _recurringJobManager = recurringJobManager;
            _configuration = configuration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var cron = _configuration["BackgroundJobs:AutoCompleteCron"] ?? "*/5 * * * *";

            _recurringJobManager.AddOrUpdate<BookingJobs>(
                AutoCompleteJobId,
                job => job.AutoComplete(),
                cron,
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

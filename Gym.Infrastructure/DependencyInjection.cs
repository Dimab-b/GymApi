using Gym.Application.Common.Interfaces;
using Gym.Domain.Bookings;
using Gym.Domain.Common;
using Gym.Domain.Members;
using Gym.Domain.Trainers;
using Gym.Infrastructure.Bookings;
using Gym.Infrastructure.Common.Services;
using Gym.Infrastructure.Members;
using Gym.Infrastructure.Persistence;
using Gym.Infrastructure.Trainers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;

namespace Gym.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString , x => x.MigrationsAssembly("Gym.Infrastructure")));

            services.AddScoped<IMemberRepository, MemberRepository>();
            services.AddScoped<ITrainerRepository, TrainerRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<IIntegrationEventPublisher, MassTransitEventPublisher>();

            var emailSettings = configuration.GetSection("EmailSettings");
            var emailFrom = emailSettings["From"] ?? throw new InvalidOperationException("EmailSettings:From is not configured.");
            var emailSmtpServer = emailSettings["SmtpServer"] ?? throw new InvalidOperationException("EmailSettings:SmtpServer is not configured.");
            var emailPort = int.TryParse(emailSettings["Port"], out var parsedEmailPort) ? parsedEmailPort : 25;

            var smtpClient = new SmtpClient(emailSmtpServer, emailPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(emailSettings["Username"], emailSettings["Password"])
            };

            services.AddFluentEmail(emailFrom)
                .AddSmtpSender(smtpClient);

            var rabbitMq = configuration.GetSection("RabbitMq");
            var rabbitHost = rabbitMq["Host"] ?? "localhost";
            var rabbitVirtualHost = rabbitMq["VirtualHost"] ?? "/";
            var rabbitUsername = rabbitMq["Username"] ?? throw new InvalidOperationException("RabbitMq:Username is not configured.");
            var rabbitPassword = rabbitMq["Password"] ?? throw new InvalidOperationException("RabbitMq:Password is not configured.");

            services.AddMassTransit(x =>
            {
                x.AddConsumers(Assembly.GetExecutingAssembly());

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitHost, rabbitVirtualHost, h =>
                    {
                        h.Username(rabbitUsername);
                        h.Password(rabbitPassword);
                    });

                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                    cfg.ConfigureEndpoints(context);
                });
            });
            return services;
        }
    }
}

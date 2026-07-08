using Gym.Application.Common.Interfaces;
using Gym.Domain.Common;
using Gym.Domain.Members;
using Gym.Domain.Trainers;
using Gym.Infrastructure.Common.Services;
using Gym.Infrastructure.Members;
using Gym.Infrastructure.Persistance;
using Gym.Infrastructure.Trainers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());
            services.AddTransient<IEmailSender, EmailSender>();
            return services;
        }
    }
}

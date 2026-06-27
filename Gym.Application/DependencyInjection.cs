using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Gym.Application.Common;
using Gym.Application.Common.Mappings;

namespace Gym.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Gym.Application.Members.Commands.RegisterMemberCommand).Assembly));

            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
            return services;
        }
    }
}

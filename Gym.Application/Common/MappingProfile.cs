using AutoMapper;
using Gym.Domain.Members;
using Gym.Application.Members.Dto_s;

namespace Gym.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Member, MemberReadDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.HeightCm, opt => opt.MapFrom(src => src.BodyMetrics != null ? src.BodyMetrics.HeightCm : (decimal?)null))
            .ForMember(dest => dest.WeightKg, opt => opt.MapFrom(src => src.BodyMetrics != null ? src.BodyMetrics.WeightKg : (decimal?)null))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.BodyMetrics != null ? src.BodyMetrics.Age : (int?)null))
            .ForMember(dest => dest.Goal, opt => opt.MapFrom(src => src.BodyMetrics != null ? src.BodyMetrics.Goal : null))
            .ForMember(dest => dest.Subscriptions, opt => opt.MapFrom(src => src.Subscriptions));

        CreateMap<Subscription, SubscriptionReadDto>()
            .ForMember(dest => dest.PriceAmount, opt => opt.MapFrom(src => src.Price.Value))
            .ForMember(dest => dest.PriceCurrency, opt => opt.MapFrom(src => src.Price.Currency))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
    }
}
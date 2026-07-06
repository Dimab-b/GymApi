using Gym.Application.Common.Interfaces;
using Gym.Domain.Members;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Members.Events
{
    public class SubscriptionPurchasedEventHandler : INotificationHandler<SubscriptionPurchasedEvent>
    {
        private readonly IEmailSender _emailSender;
        private readonly IMemberRepository _memberRepository;

        public SubscriptionPurchasedEventHandler(IEmailSender emailSender , IMemberRepository memberRepository)
        {
            _emailSender = emailSender;
            _memberRepository = memberRepository;
        }

        public async Task Handle(SubscriptionPurchasedEvent purchasedEvent, CancellationToken cancellationToken = default)
        {
            var member = await _memberRepository.GetByIdAsync(purchasedEvent.MemberId , cancellationToken) ?? throw new ArgumentException("No member found"); 

            var sb = new StringBuilder();
                    
            sb.AppendLine($"{member.Name} ,Thank for purchasing a subscription of our Gym");
            sb.AppendLine($"Start date of subscription: {purchasedEvent.StartDate}");
            sb.AppendLine($"End date of subscription: {purchasedEvent.EndDate}");
            sb.AppendLine("Enjoy your training");
                    

            var body = sb.ToString();

            await _emailSender.SendAsync(member.Email.Value , "Purchased subscription" , body);
        }
    }
}

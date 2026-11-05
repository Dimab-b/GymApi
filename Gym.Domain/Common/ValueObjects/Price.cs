using Gym.Domain.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Domain.Common.VO
{
    public record Price
    {
        public decimal Value { get;}
        public string Currency { get;}
        public Price(decimal value , string currency)
        {
            if (value <= 0)
                throw new InvalidPriceException();

            if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
                throw new InvalidCurrencyException();

            Value = value;
            Currency = currency.Trim().ToUpperInvariant();
        }
    }
    
    
}

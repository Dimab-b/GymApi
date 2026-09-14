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
                throw new ArgumentException("Value must be greater than 0");

            if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
                throw new ArgumentException("Currency must be in 3 letters");

            Value = value;
            Currency = currency.Trim().ToUpperInvariant();
        }
    }
    
    
}

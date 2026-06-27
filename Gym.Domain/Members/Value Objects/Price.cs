using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Domain.Members.Value_Objects
{
    public record Price
    {
        public decimal Value { get;}
        public string Currency { get;}
        public Price(decimal value , string currency)
        {
            if (value <= 0)
                throw new Exception("Value must be greater than 0");

            if (currency.Length > 3 || string.IsNullOrWhiteSpace(currency))
                throw new Exception("Currency must be in 3 letters");

            Value = value;
            Currency = currency;
        }
    }
    
    
}

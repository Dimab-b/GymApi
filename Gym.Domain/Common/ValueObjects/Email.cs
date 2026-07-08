using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Domain.Common.VO
{
    public record Email
    {
        public string Value { get; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains("@"))
                throw new ArgumentException("Invalid email format");

            Value = value;
        }
    }
}

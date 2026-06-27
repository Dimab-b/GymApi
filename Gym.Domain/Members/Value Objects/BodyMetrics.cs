using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Domain.Members.Value_Objects
{
    public record BodyMetrics
    {
        public decimal HeightCm { get; }
        public decimal WeightKg { get; }
        public int Age { get; }
        public string Goal { get; }

        public BodyMetrics(decimal heightCm, decimal weightKg, int age, string goal)
        {
            if (heightCm <= 0 || weightKg <= 0 || age <= 0)
                throw new ArgumentException("Metrics must be positive values.");

            if (string.IsNullOrWhiteSpace(goal))
                throw new ArgumentException("Goal cannot be empty.");

            HeightCm = heightCm;
            WeightKg = weightKg;
            Age = age;
            Goal = goal;
        }
    }
}

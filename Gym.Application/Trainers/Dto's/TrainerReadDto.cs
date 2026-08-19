using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Trainers.Dto_s
{
    public record TrainerReadDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Specialization { get; set; } = null!;
        public string Email { get; set; } = null!;
        public decimal Price { get; set; }
        public string Currency { get; set; } = null!;
    }
}

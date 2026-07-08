using Gym.Domain.Common;
using Gym.Domain.Common.VO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Domain.Trainers
{
    public class Trainer : Entity , IAggregateRoot
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public Email Email { get; private set; }
        public string Specialization { get; private set; }
        public Price SessionPrice { get; private set; }
        public bool IsActive { get; private set; } = true;



        private Trainer(){ }


        private Trainer(Guid id ,string name , Email email , string specialization , Price sessionPrice)
        {
            Id = id;
            Name = name;
            Email = email;
            Specialization = specialization;
            SessionPrice = sessionPrice;
            IsActive = true;
        }

        public static Trainer Create(string name, Email email, string specialization, Price sessionPrice)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), "Trainer name cannot be empty");

            if (sessionPrice.Value < 0)
                throw new ArgumentException("Session price cannot be negative");

            var trainer = new Trainer(Guid.NewGuid(), name, email, specialization, sessionPrice);

            trainer.AddDomainEvent(new TrainerCreatedEvent());

            return trainer;
        }

        public void ChangePrice(Price price)
        {
            if (!this.IsActive)
                throw new ArgumentException("Inactive trainer can't change price");
            this.SessionPrice = price;
        }


        public void Deactivate()
        {
            if (this.IsActive == false)
                throw new InvalidOperationException("Trainer is already inactive");

            this.IsActive = false;

            AddDomainEvent(new TrainerDeactivatedEvent());
        }


        public void UpdateProfile(string name, string specialization)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), "Trainer name cannot be empty");

            if (string.IsNullOrWhiteSpace(specialization))
                throw new ArgumentNullException(nameof(specialization), "Specialization must be filled");


            this.Name = name;
            this.Specialization = specialization;
        }


    }
}

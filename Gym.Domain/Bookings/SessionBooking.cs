using Gym.Domain.Common;
using Gym.Domain.Common.VO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Domain.Bookings
{
    public enum BookingStatus {Scheduled , Completed , Cancelled};
    public class SessionBooking : Entity , IAggregateRoot
    {
        public Guid Id { get; private set; }
        public Guid MemberId { get; private set; }
        public Guid TrainerId { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public BookingStatus Status { get; private set; }
        public Price FixedTrainerPrice { get; private set; }

        private SessionBooking() { }

        private SessionBooking(Guid id , Guid memberId , Guid trainerId , DateTime startTime , DateTime endTime , BookingStatus status , Price price )
        {
            Id = id;
            MemberId = memberId;
            TrainerId = trainerId;
            StartTime = startTime;
            EndTime = endTime;
            Status = status;
            FixedTrainerPrice = price;
        }


        public static SessionBooking Create(Guid memberId ,Guid trainerId , DateTime startTime, Price price)
        {
            if (startTime < DateTime.UtcNow)
                throw new ArgumentException("Time of workout cannot be in the past");
            if (startTime < DateTime.UtcNow.AddHours(12))
                throw new ArgumentException("You must book a workout at least 12 hours in advance.");
            if (startTime > DateTime.UtcNow.AddDays(30))
                throw new ArgumentException("You cannot book a workout more than 30 days in advance.");


            var session = new SessionBooking(Guid.NewGuid() , memberId , trainerId , startTime , startTime.AddHours(1) , BookingStatus.Scheduled , price);

            session.AddDomainEvent(new SessionScheduledEvent(session.Id , session.MemberId ,session.TrainerId, session.StartTime));

            return session;
        }


        public void Cancel()
        {
            if (this.Status == BookingStatus.Completed)
                throw new InvalidOperationException("You cannot cancel the session that already has been completed");
            if (this.StartTime < DateTime.UtcNow.AddDays(1))
                throw new InvalidOperationException("Session cannot be canceled less than 24 hours in advance");
            if (this.Status == BookingStatus.Cancelled)
                return;
            this.Status = BookingStatus.Cancelled;

            this.AddDomainEvent(new SessionCancelledEvent(this.Id , this.MemberId , this.TrainerId));
        }

        public void Complete()
        {
            if (this.Status == BookingStatus.Cancelled)
                throw new InvalidOperationException("You cannot complete a session that was cancelled");
            if (this.EndTime > DateTime.UtcNow)
                throw new InvalidOperationException("You cannot complete a session before its end time");
            if (this.Status == BookingStatus.Completed)
                return;
            this.Status = BookingStatus.Completed;
        }
    }
}

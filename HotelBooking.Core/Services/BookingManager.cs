using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelBooking.Core
{
    public class BookingManager : IBookingManager
    {
        private IRepository<Booking> bookingRepository;
        private IRepository<Room> roomRepository;

        // Constructor injection
        public BookingManager(IRepository<Booking> bookingRepository, IRepository<Room> roomRepository)
        {
            this.bookingRepository = bookingRepository;
            this.roomRepository = roomRepository;
        }

        public Booking GetBooking(int id)
        {
            IEnumerable<Booking> allBookings = bookingRepository.GetAll();
            return allBookings.First(b => b.Id == id);
        }

        public bool CreateBooking(Booking booking)
        {

            int roomId = FindAvailableRoom(booking.StartDate, booking.EndDate);
            if (roomId >= 0)
            {
                booking.RoomId = roomId;
                booking.IsActive = true;
                bookingRepository.Add(booking);
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool CancelBooking(int id)
        {
            Booking booking = bookingRepository.Get(id);
            if (booking != null && booking.IsActive)
            {
                //Checking if the booking is meeting the threshold for cansellation (24 hours)

                if (DateTime.Today <= booking.StartDate.AddHours(-24))
                {
                    bookingRepository.Edit(booking);
                    return true;
                }
                throw new InvalidTimeZoneException("Booking cannot be cancelled");
            }

            else
            {
                throw new EntryPointNotFoundException("Booking not found");
            }
        }

        public int FindAvailableRoom(DateTime startDate, DateTime endDate)
        {
            if (startDate <= DateTime.Today || startDate > endDate)
                throw new ArgumentException("The start date cannot be in the past or later than the end date.");

            var activeBookings = bookingRepository.GetAll().Where(b => b.IsActive);
            foreach (var room in roomRepository.GetAll())
            {
                var activeBookingsForCurrentRoom = activeBookings.Where(b => b.RoomId == room.Id);
                if (activeBookingsForCurrentRoom.All(b => startDate < b.StartDate &&
                    endDate < b.StartDate || startDate > b.EndDate && endDate > b.EndDate))
                {
                    return room.Id;
                }
            }
            return -1;
        }
        public List<DateTime> GetFullyOccupiedDates(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("The start date cannot be later than the end date.");

            List<DateTime> fullyOccupiedDates = new List<DateTime>();
            var bookings = bookingRepository.GetAll();
            var rooms = roomRepository.GetAll().Count();
            var bookingCounts = new Dictionary<DateTime, int>();
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                bookingCounts[date] = bookings.Count(b => date >= b.StartDate && date <= b.EndDate);
                if (bookingCounts[date] >= rooms)
                {
                    fullyOccupiedDates.Add(date);
                }
            }
            return fullyOccupiedDates;
        }
    }
}

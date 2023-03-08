using System;
using System.Collections;
using System.Collections.Generic;
using HotelBooking.Core;
using HotelBooking.Infrastructure.Repositories;
using HotelBooking.UnitTests.Fakes;
using Microsoft.Identity.Client;
using Moq;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private IBookingManager bookingManager;
        private Mock<IRepository<Room>> fakeRoomRepository;
        private Mock<IRepository<Booking>> fakeBookingRepository;
        public BookingManagerTests(){

            // Rooms for the setup of the fake room repository
            var rooms = new List<Room>
            {
                new Room { Id=1, Description="A" },
                new Room { Id=2, Description="B" },
            };

            // Booking for the setup of the fake booking repository
            var bookings = new List<Booking>
            {
                new Booking {Id=1, StartDate=DateTime.Today.AddDays(10), EndDate=DateTime.Today.AddDays(20), IsActive=true, RoomId=1},
                new Booking {Id=2, StartDate=DateTime.Today.AddDays(10), EndDate=DateTime.Today.AddDays(20), IsActive=true, RoomId=2},
                new Booking {Id=3, StartDate=DateTime.Today.AddDays(100), EndDate=DateTime.Today.AddDays(101), IsActive=true, RoomId=2},
                new Booking {Id=4, StartDate=DateTime.Today.AddDays(-10), EndDate=DateTime.Today.AddDays(-5), IsActive=true, RoomId=2},
            };
            #region FakeRoomRepository_Setup
            // Create fake RoomRepository. 
            fakeRoomRepository = new Mock<IRepository<Room>>();

            // Implement fake GetAll() method.
            fakeRoomRepository.Setup(x => x.GetAll()).Returns(rooms);
            
            fakeRoomRepository.Setup(x =>
            x.Get(It.IsInRange(1, 2, Moq.Range.Inclusive))).Returns(rooms[0]);
            #endregion
            #region fakeBookingRepository_Setup
            // Create fake BookingRepository.
            fakeBookingRepository = new Mock<IRepository<Booking>>();

            // Implement fake GetAll() method.
            fakeBookingRepository.Setup(x => x.GetAll()).Returns(bookings);

            // Create fake BookingManager.
            bookingManager = new BookingManager(fakeBookingRepository.Object, fakeRoomRepository.Object);

            // Implement fake Get() method.
            fakeBookingRepository.Setup(x =>
                x.Get(
                    It.IsInRange(1, 4, Moq.Range.Inclusive))).Returns(bookingManager.GetBooking);


            #endregion
        }

        [Fact]
        public void GetBooking_BookingExists_ReturnBooking() 
        {
            // Arrange
            int id = 1;
            Booking expected = new () { Id = 1, StartDate = DateTime.Today.AddDays(10), EndDate = DateTime.Today.AddDays(20), IsActive = true, RoomId = 1 };

            // Act
            Booking actual = bookingManager.GetBooking(id);

            // Assert - Verifying that all properties has changed
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.EndDate, actual.EndDate);
            Assert.Equal(expected.StartDate, actual.StartDate);
            Assert.Equal(expected.RoomId, actual.RoomId);
        }

        [Fact]
        public void GetBooking_BookingDoesNotExists_ThrowsException()
        {
            // Arrange
            int idToTest = 99;
            // Act
            Action act = () => bookingManager.GetBooking(idToTest);
            // Assert
            Assert.Throws<InvalidOperationException>(act);
        }

        [Fact]
        public void CancleBooking_BookingCanBeCancel_ReturnTrue()
        {
            // Arrange - Using booking with ID 3, already setup in the mock repository
            var bookingId = 3;
            
            // Act
            var result = bookingManager.CancelBooking(bookingId);
            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(99, typeof(EntryPointNotFoundException))]
        [InlineData(4, typeof(InvalidTimeZoneException))]
        public void CancleBooking_BookingCannotBeCancel_ThrowException(int bookingId, Type expected) 
        {
            // Arrange
            int idToTest = bookingId;
            // Act
            Action act = () => bookingManager.CancelBooking(idToTest);
            // Assert
            Assert.Throws(expected, act);

        }
    

        [Fact]
        public void FindAvailableRoom_StartDateNotInTheFuture_ThrowsArgumentException()
        {
            // Arrange
            DateTime date = DateTime.Today;

            // Act
            Action act = () => bookingManager.FindAvailableRoom(date, date);
            
            // Assert
            Assert.Throws<ArgumentException>(act);
        }
        
        public static IEnumerable<object[]> OccupiedTestData()
        {
            //A vacant period before the known bookings 
            yield return new object[] { DateTime.Today, DateTime.Today.AddDays(5), 0 };
            
            //The Vacant period plus the start of a booking
            yield return new object[] { DateTime.Today, DateTime.Today.AddDays(10), 1 };

            //A period spanning all known bookings
            yield return new object[] { DateTime.Today.AddDays(10), DateTime.Today.AddDays(20), 11 }; 

            //A Intersecting period in start to middle
            yield return new object[] { DateTime.Today.AddDays(5), DateTime.Today.AddDays(15), 6 };

            //A Intersecting period in middle to end
            yield return new object[] { DateTime.Today.AddDays(15), DateTime.Today.AddDays(25), 6 };

            //A vacant period after the known bookings 
            yield return new object[] { DateTime.Today.AddDays(21), DateTime.Today.AddDays(25), 0 }; 
        }

        [Theory]
        [MemberData(nameof(OccupiedTestData))]
        public void GetFullyOccupiedDates_VacantPeriod_ReturnsNumberOfOccupiedDates(DateTime startDate, DateTime endDate, int numOfOccupiedDates)
        {
            
            // Act
            var occupiedDates = bookingManager.GetFullyOccupiedDates(startDate, endDate);

            // Assert
            Assert.Equal(numOfOccupiedDates, occupiedDates.Count);
        }

        [Fact]
        public void FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            // Act
            int roomId = bookingManager.FindAvailableRoom(date, date);
            // Assert
            Assert.NotEqual(-1, roomId);
        }

        [Theory]
        [ClassData(typeof(BookingTestData))]
        public void CreateBooking_BookingIsCreated_ReturnTrueOrFalseIfBookingIsCreated(DateTime start, DateTime end, bool expected) 
        {
            //Arrange
            Booking booking = new() { StartDate = start, EndDate = end };
            //Act
            bool result = bookingManager.CreateBooking(booking);
            //Assert
            Assert.Equal(expected, result);
        }

        
        [Theory]
        [MemberData(nameof(Data))]
        public void CreateBooking_BookingDateNotInvalid_ThrowsArgumentException(DateTime start, DateTime end)
        {
            //Arrange
            Booking booking = new() { StartDate= start, EndDate= end };
            //Act
            Action act = () => bookingManager.CreateBooking(booking);
            //Assert
            Assert.Throws<ArgumentException>(act);
        }

        public static IEnumerable<object[]> Data =>
        new List<object[]>
        {
            new object[] { DateTime.Today.AddDays(-10), DateTime.Today.AddDays(1) },
            new object[] { DateTime.Today.AddDays(10), DateTime.Today.AddDays(1) }
        };

    }

    public class BookingTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] {DateTime.Today.AddDays(10), DateTime.Today.AddDays(20), false };
            yield return new object[] {DateTime.Today.AddDays(1), DateTime.Today.AddDays(2), true };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using HotelBooking.Core;
using HotelBooking.Infrastructure;
using HotelBooking.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HotelBooking.IntegrationTests
{
    public class BookingManagerTests : IDisposable
    {
        // This test class uses a separate Sqlite in-memory database. While the
        // .NET Core built-in in-memory database is not a relational database,
        // Sqlite in-memory database is. This means that an exception is thrown,
        // if a database constraint is violated, and this is a desirable behavior
        // when testing.

        SqliteConnection connection;
        BookingManager bookingManager;

        public BookingManagerTests()
        {
            connection = new SqliteConnection("DataSource=:memory:");

            // In-memory database only exists while the connection is open
            connection.Open();

            // Initialize test database
            var options = new DbContextOptionsBuilder<HotelBookingContext>()
                            .UseSqlite(connection).Options;
            var dbContext = new HotelBookingContext(options);
            IDbInitializer dbInitializer = new DbInitializer();
            dbInitializer.Initialize(dbContext);

            // Create repositories and BookingManager
            var bookingRepos = new BookingRepository(dbContext);
            var roomRepos = new RoomRepository(dbContext);
            bookingManager = new BookingManager(bookingRepos, roomRepos);
        }

        public void Dispose()
        {
            // This will delete the in-memory database
            connection.Close();
        }

        [Fact]
        public void FindAvailableRoom_RoomNotAvailable_RoomIdIsMinusOne()
        {
            // Act
            var roomId = bookingManager.FindAvailableRoom(DateTime.Today.AddDays(8), DateTime.Today.AddDays(8));
            // Assert
            Assert.Equal(-1, roomId);
        }



        [Fact]
        public void GetBooking_BookingExists_ReturnBooking()
        {
            // Arrange
            int id = 1;
            Booking expected = new() { Id = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(2), IsActive = true, RoomId = 1 };
            // Act
            Booking actual = bookingManager.GetBooking(id);

            // Assert
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
            // Arrange
            var bookingId = 3;

            // Act
            var result = bookingManager.CancelBooking(bookingId);
            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(99, typeof(EntryPointNotFoundException))]
        [InlineData(1, typeof(InvalidTimeZoneException))]
        public void CancleBooking_BookingCannotBeCancel_ThrowException(int bookingId, Type expected)
        {
            // Arrange
            int idToTest = bookingId;
            Type exceptionToExpect = expected;
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
            //Bookings in the DB at init
            //StartDate = Today+4days
            
           /* 1 { StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(2), IsActive = true, CustomerId = 1, RoomId = 1 },
              2 { StartDate = date, EndDate = date.AddDays(14), IsActive = true, CustomerId = 1, RoomId = 1 },
              3 { StartDate = date, EndDate = date.AddDays(14), IsActive = true, CustomerId = 2, RoomId = 2 },
              4 { StartDate = date, EndDate = date.AddDays(14), IsActive = true, CustomerId = 1, RoomId = 3 }*/


            //A period spanning all known bookings
            yield return new object[] { DateTime.Today, DateTime.Today.AddDays(14), 11 };

            //A Intersecting period in start to middle
            yield return new object[] { DateTime.Today, DateTime.Today.AddDays(10), 7 };

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
            int customerId = 1;
            int roomId = 1;

            //Arrange
            Booking booking = new() { StartDate = start, EndDate = end, RoomId = roomId, CustomerId = customerId, IsActive=true };
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
            Booking booking = new() { StartDate = start, EndDate = end };
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
            yield return new object[] { DateTime.Today.AddDays(4), DateTime.Today.AddDays(14), false };
            yield return new object[] { DateTime.Today.AddHours(1), DateTime.Today.AddDays(14), false };
            yield return new object[] { DateTime.Today.AddDays(1), DateTime.Today.AddDays(2), true };
            yield return new object[] { DateTime.Today.AddDays(50), DateTime.Today.AddDays(60), true };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
          
}


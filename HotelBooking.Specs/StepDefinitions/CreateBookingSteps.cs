using HotelBooking.Core;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Specs.StepDefinitions
{
    [Binding]
    public class CreateBookingSteps
    {
        private IBookingManager bookingManager;
        private Mock<IRepository<Room>> fakeRoomRepository;
        private Mock<IRepository<Booking>> fakeBookingRepository;

        private DateTime start;
        private DateTime end;
        private static int customerId = 1;
        private static int roomID1 = 1;
        private static int roomID2 = 2;
        private static int id = 2;
        private bool result;
        private static int TenDays = 10;
        private static int TwentyDays = 20;
        private static int OneHundredeDays = 100;
        private static int Minus10Days = -10;
        private static int Minus5Days = -5;
        
        public CreateBookingSteps()
        {
            start = DateTime.Today.AddDays(TenDays);
            end = DateTime.Today.AddDays(TwentyDays);

            // Rooms for the setup of the fake room repository
            var rooms = new List<Room>
            {
                new Room { Id=roomID1, Description="A" },
                new Room { Id=roomID2, Description="B" }
            };

            // Booking for the setup of the fake booking repository
            var bookings = new List<Booking>
            {
                new Booking {Id=1, StartDate=DateTime.Today.AddDays(TenDays), EndDate=DateTime.Today.AddDays(TwentyDays), IsActive=true, RoomId=roomID1},
                new Booking {Id=2, StartDate=DateTime.Today.AddDays(TenDays), EndDate=DateTime.Today.AddDays(TwentyDays), IsActive=true, RoomId=roomID2},
                new Booking {Id=3, StartDate=DateTime.Today.AddDays(OneHundredeDays), EndDate=DateTime.Today.AddDays(OneHundredeDays+1), IsActive=true, RoomId=roomID2},
                new Booking {Id=4, StartDate=DateTime.Today.AddDays(Minus10Days), EndDate=DateTime.Today.AddDays(Minus5Days), IsActive=true, RoomId=roomID2},
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

            bookingManager = new BookingManager(fakeBookingRepository.Object, fakeRoomRepository.Object);


        }

        [Given(@"start date before occupied time")]
        public void GivenStartDateBeforeOccupiedTime()
        {
            start = DateTime.Today.AddDays(TenDays - 3);
        }

        [Given(@"end date before occupied time")]
        public void GivenEndDateBeforeOccupiedTime()
        {
            end = DateTime.Today.AddDays(TenDays - 2);
        }

        [When(@"creating a booking")]
        public void WhenCreatingABooking()
        {
            Booking booking = new Booking() { CustomerId = customerId, StartDate = start, EndDate = end, RoomId = roomID1, Id = id };

            result = bookingManager.CreateBooking(booking);
        }

        [Then(@"the booking should be created")]
        public void ThenTheBookingShouldBeCreated()
        {
            Assert.True(result);
        }

        [Given(@"start date after occupied time")]
        public void GivenStartDateAfterOccupiedTime()
        {
            start = DateTime.Today.AddDays(TwentyDays + 2);
        }

        [Given(@"end date after occupied time")]
        public void GivenEndDateAfterOccupiedTime()
        {
            end = DateTime.Today.AddDays(TwentyDays + 3);
        }

        [Then(@"the booking should not be created")]
        public void ThenTheBookingShouldNotBeCreated()
        {
            Assert.False(result);
        }

        [Given(@"start date in occupied time")]
        public void GivenStartDateInOccupiedTime()
        {
            start = DateTime.Today.AddDays(TenDays);
        }

        [Given(@"end date in occupied time")]
        public void GivenEndDateInOccupiedTime()
        {
            end = DateTime.Today.AddDays(TwentyDays);
        }

        [Given(@"start date after, and > end date")]
        public void GivenStartDateAfterAndEndDate()
        {
            start = DateTime.Today.AddDays(TenDays + 4);
        }
    }
}

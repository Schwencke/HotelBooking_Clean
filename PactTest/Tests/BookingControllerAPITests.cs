using HotelBooking.WebApi.Controllers;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using PactNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using PactNet.Matchers;
using System.Net;
using Consumer;
using ConsumerBooking.Tests;
using HotelBooking.Core;
using Moq;

namespace PactTest.Tests
{
    public class BookingControllerAPITests
    {
        private BookingsController controller;
        private Mock<IRepository<Booking>> fakeBookingRepo;
        private IPactBuilderV3 pact;
        private readonly ApiClient ApiClient;
       // private readonly int port = 5001;
        private readonly int port = 60472;
        private readonly List<Booking> _bookings;
        private readonly object _badBooking;
        private readonly string _editBooking;
        private readonly string _editNotFoundBooking;

        public BookingControllerAPITests(ITestOutputHelper output)
        {

            //Creating data for our fakes

            DateTime startDate = DateTime.Parse("2023-08-01T00:00:00+01:00");
            DateTime endDate = DateTime.Parse("2023-08-01T00:00:00+01:00");
            Customer jane = new() { Name = "Jane Doe", Email = "jd@gmail.com" };
            Customer john = new() { Name = "John Smith", Email = "js@gmail.com" };

            _bookings = new List<Booking>()
            {
                new Booking{Id=1, StartDate = startDate, EndDate= endDate, IsActive = true, CustomerId= 1, Customer = jane },
                new Booking{Id=2, StartDate = startDate, EndDate= endDate, IsActive = true, CustomerId= 2, Customer = john }

            };


            _badBooking = new { status = "Bad Object" };
            _editBooking = @"
                            {
                              ""Id"": 2,
                              ""StartDate"": ""2023-08-01T00:00:00+01:00"",
                              ""EndDate"": ""2023-08-01T00:00:00+01:00"",
                              ""IsActive"": true,
                              ""CustomerId"": 1,
                              ""Customer"": {
                                ""Name"": ""Jane Doe"",
                                ""Email"": ""jd@gmail.com""
                              }
}";
            _editNotFoundBooking = @"
                                {
                                  ""Id"": 2,
                                  ""StartDate"": ""2023-08-01T00:00:00+01:00"",
                                  ""EndDate"": ""2023-08-01T00:00:00+01:00"",
                                  ""IsActive"": true,
                                  ""CustomerId"": 2,
                                  ""Customer"": {
                                    ""Name"": ""John Smith"",
                                    ""Email"": ""js@gmail.com""
                                  }
                                }";

            var Config = new PactConfig
            {
                PactDir = Path.Join("..", "..", "..", "pacts"),

                LogLevel = PactLogLevel.Debug,
                Outputters = (new[] { new XUnitOutput(output) }),
                DefaultJsonSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            };
            pact = Pact.V3("ApiClient", "BookingController", Config).WithHttpInteractions(port);
            ApiClient = new ApiClient(new System.Uri($"http://localhost:{port}"));
        }

        [Fact]
        public async void GetAllBookingsReturnsOkWithData()
        {
            // Arange
            pact.UponReceiving("A valid request for all bookings")
                    .Given("There is data")
                    .WithRequest(HttpMethod.Get, "/api/bookings")
                .WillRespond()
                    .WithStatus(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithJsonBody(new TypeMatcher(_bookings));

            await pact.VerifyAsync(async ctx =>
            {
                var response = await ApiClient.GetAllBookings();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }

        [Fact]
        public async void GetAllBookingsReturnsOkWithNoData()
        {
            // Arange
            pact.UponReceiving("A valid request for all bookings")
                    .Given("There is no data")
                    .WithRequest(HttpMethod.Get, "/api/bookings")
                .WillRespond()
                    .WithStatus(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithJsonBody(new TypeMatcher(new List<Booking>()));

            await pact.VerifyAsync(async ctx =>
            {
                var response = await ApiClient.GetAllBookings();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }

        [Fact]
        public async void GetBookingFoundAndReturnsOk()
        {
            // Arange
            int id = 1;
            HttpStatusCode code = HttpStatusCode.NotFound;
            pact.UponReceiving("A valid request for a booking")
                    .Given("There is data")
                    .WithRequest(HttpMethod.Get, $"/api/booking/{id}")
                .WillRespond()
                    .WithStatus(code)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithJsonBody(new TypeMatcher(
                        _bookings.Where(bok => bok.Id == id)
                        ));

            await pact.VerifyAsync(async ctx =>
            {
                var response = await ApiClient.GetBookingById(id);
                Assert.Equal(code, response.StatusCode);
            });
        }

        [Fact]
        public async void GetBookingNotFoundAndReturnsNotFound()
        {
            // Arange
            int id = 3;
            HttpStatusCode code = HttpStatusCode.NotFound;
            pact.UponReceiving("A valid request for a booking")
                    .Given("There is no data")
                    .WithRequest(HttpMethod.Get, $"/api/booking/{id}")
                .WillRespond()
                    .WithStatus(code)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithJsonBody(new TypeMatcher(
                        _bookings.Where(bok => bok.Id == id)
                        ));

            await pact.VerifyAsync(async ctx =>
            {
                var response = await ApiClient.GetBookingById(id);
                Assert.Equal(code, response.StatusCode);
            });
        }


        [Fact]
        public async void DeleteBookingOkReturnsOk()
        {
            // Arange
            int id = 1;
            HttpStatusCode code = HttpStatusCode.NoContent;
            pact.UponReceiving("A valid request for deletion of a booking")
                    .Given("There is data")
                    .WithRequest(HttpMethod.Delete, $"/api/booking/{id}")
                .WillRespond()
                    .WithStatus(code);

            await pact.VerifyAsync(async ctx =>
            {
                var response = await ApiClient.DeleteBookingById(id);
                Assert.Equal(code, response.StatusCode);
            });
        }

        [Fact]
        public async void DeleteBookingReturnsNotFound()
        {
            // Arange
            int id = 3;
            HttpStatusCode code = HttpStatusCode.NotFound;
            pact.UponReceiving("A valid request for deletion of a booking")
                    .Given("There is no booking with id")
                    .WithRequest(HttpMethod.Delete, $"/api/booking/{id}")
                .WillRespond()
                    .WithStatus(code);

            await pact.VerifyAsync(async ctx =>
            {
                var response = await ApiClient.DeleteBookingById(id);
                Assert.Equal(code, response.StatusCode);
            });
        }

        [Fact]
        public async void EditBookingWithBadBodyFormatReturnBadRequest()
        {
            // Arange
            int id = 1;
            HttpStatusCode code = HttpStatusCode.BadRequest;
            pact.UponReceiving("A invalid request for edit of a booking")
                    .Given("the body provided with the request is bad")
                    .WithRequest(HttpMethod.Put, $"/api/booking/{id}")
                    .WithJsonBody(_badBooking)
                .WillRespond()
                    .WithStatus(code);

            await pact.VerifyAsync(async ctx =>
            {
                var response = await ApiClient.EditBookingById(id, JsonConvert.SerializeObject(_badBooking));
                Assert.Equal(code, response.StatusCode);
            });
        }

        [Fact]
        public async void EditBookingReturnsNotFound()
        {
            // Arange
            int id = 3;
            HttpStatusCode code = HttpStatusCode.NotFound;
            pact.UponReceiving("A valid request for edit of a booking")
                    .Given("There is no booking with id")
                    .WithRequest(HttpMethod.Put, $"/api/booking/{id}")
                    .WithJsonBody(_editBooking, "application/json;charset=utf-8")
                .WillRespond()
                    .WithStatus(code);

            await pact.VerifyAsync(async ctx =>
            {
                var response = await ApiClient.EditBookingById(id, _editBooking);
                Assert.Equal(code, response.StatusCode);
            });
        }

        [Fact]
        public async void EditBookingReturnsOkNoContent()
        {
            // Arange
            int id = 2;
            HttpStatusCode code = HttpStatusCode.NoContent;
            pact.UponReceiving("A valid request for edit of a booking")
                    .Given("There is data")
                    .WithRequest(HttpMethod.Put, $"/api/booking/{id}")
                    .WithJsonBody(_editBooking, "application/json;charset=utf-8")
                .WillRespond()
                    .WithStatus(code);

            await pact.VerifyAsync(async ctx =>
            {
                var response = await ApiClient.EditBookingById(id, _editBooking);
                Assert.Equal(code, response.StatusCode);
            });
        }

    }


}

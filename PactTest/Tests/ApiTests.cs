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

namespace PactTest.Tests
{
    public class ApiTests
    {
        private IPactBuilderV3 pact;
        private readonly ApiClient ApiClient;
        private readonly int port = 5001;
        private readonly List<object> _bookings;

        public ApiTests(ITestOutputHelper output)
        {

            _bookings = new List<object>()
            {
                new
            {
                id = 1,
                startDate = "2023-08-01T00:00:00+01:00",
                endDate = "2023-08-10T00:00:00+01:00",
                isActive = true,
                customerId = 1,
                customer = new
                {
                    name = "Jane Doe",
                    email = "jd@gmail.com"
                }
            },
             new
             {
                 id = 2,
                 startDate = "2023-08-01T00:00:00+01:00",
                 endDate = "2023-08-10T00:00:00+01:00",
                 isActive = true,
                 customerId = 2,
                 customer = new
                 {
                     name = "John Smith",
                     email = "js@gmail.com"
                 }
             }
            };

            var Config = new PactConfig
            {
                PactDir = Path.Join("..", "..", "..", "..", "..", "pacts"),
               
                Outputters = (new[] { new XUnitOutput(output) }),
                DefaultJsonSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            };

            // pact = Pact.V3("ApiClient", "ProductService", Config).UsingNativeBackend(port);
            pact = Pact.V3("ApiClient", "ProductService", Config).WithHttpInteractions(port);
            ApiClient = new ApiClient(new System.Uri($"https://localhost:{port}"));
        }

        [Fact]
        public async void GetAllProducts()
        {
            // Arange
            pact.UponReceiving("A valid request for all products")
                    .Given("There is data")
                    .WithRequest(HttpMethod.Get, "/api/products")
                .WillRespond()
                    .WithStatus(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithJsonBody(new TypeMatcher(_bookings));

            await pact.VerifyAsync(async ctx => {
                var response = await ApiClient.GetAllBookings();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }

        [Fact]
        public async void GetProduct()
        {
            // Arange
            pact.UponReceiving("A valid request for a product")
                    .Given("There is data")
                    .WithRequest(HttpMethod.Get, "/api/product/10")
                .WillRespond()
                    .WithStatus(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithJsonBody(new TypeMatcher(_bookings[1]));

            await pact.VerifyAsync(async ctx => {
                var response = await ApiClient.GetBookingById(2);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }

    }


}

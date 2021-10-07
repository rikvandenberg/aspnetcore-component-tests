using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Threading.Tasks;
using Api.ApiLayer;
using Api.BusinessLayer;
using Api.DataLayer;
using Api.Shopify;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;

namespace Api.Tests
{
    public class OrderApiComponentTest
    {
        private readonly WebApplicationFactory<Program> _webApplicationFactory;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly MockHttpMessageHandler MockHttpMessageHandler = new();

        public OrderApiComponentTest(ITestOutputHelper testOutputHelper)
        {
            _webApplicationFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(CustomizeWebHostBuilder);
            _testOutputHelper = testOutputHelper;
        }

        private void CustomizeWebHostBuilder(IWebHostBuilder webHostBuilder)
        {
            webHostBuilder
                .ConfigureAppConfiguration((context, configuration) =>
                {
                    string folder = Path.GetDirectoryName(GetType().Assembly.Location);
                    string file = "ComponentTest/component.test.json";
                    configuration.AddJsonFile(Path.Combine(folder, file));
                })
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddXunit(_testOutputHelper);
                })
                .ConfigureTestServices(services =>
                {
                    services.AddHttpClient<IProductsService, ShopifyProductsClient>()
                       .ConfigurePrimaryHttpMessageHandler(_ => MockHttpMessageHandler);
                    services.AddSingleton((serviceProvider) =>
                    {
                        var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>().UseInMemoryDatabase("orders");
                        return optionsBuilder.Options;
                    });
                });
        }

[Fact]
public async Task ComponentTest()
{
    // Arrange
    MockHttpMessageHandler
        .When(HttpMethod.Get, "https://shopify/api/v1/products/admin/api/2021-07/products/APPLE_IPHONE.json")
        .WithHeaders(new Dictionary<string, string>
        {
            ["X-Shopify-Access-Token"] = "secret"
        })
        .Respond(MediaTypeNames.Application.Json, File.ReadAllText("Shopify/Examples/APPLE_IPHONE.json"));

    // Act #1
    HttpResponseMessage getAllResponse = await SystemUnderTest.GetAsync("/api/v1/orders");
    var orders = await getAllResponse.Content.ReadFromJsonAsync<Order[]>();
    orders.Length.Should().Be(0);

    // Act #2
    HttpResponseMessage createResponse = await SystemUnderTest.PostAsync(
        "/api/v1/orders", 
        JsonContent.Create(new CreateOrderDto()
        {
            ProductNumbers = new[] { "APPLE_IPHONE" },
            UserId = Guid.NewGuid(),
            TotalAmount = 495m,
        }));

    // Act #3
    getAllResponse = await SystemUnderTest.GetAsync("/api/v1/orders");


    // Assert
    createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    orders = await getAllResponse.Content.ReadFromJsonAsync<Order[]>();
    orders.Length.Should().Be(1);
}


        [Fact]
        public async Task When_application_starts_up_should_load_config_from_json_file()
        {
            // Arrange
            IConfiguration configuration = _webApplicationFactory.Services.GetRequiredService<IConfiguration>();

            // Act/Assert
            configuration["Shopify:ApiKey"].Should().Be("secret");
        }

        [Fact]
        public async Task When_GET_is_called_on_the_api_should_return_200_OK()
        {
            // Arrange

            // Act
            HttpResponseMessage response = await SystemUnderTest.GetAsync("/api/v1/orders");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        public HttpClient SystemUnderTest =>
            _webApplicationFactory.CreateClient();
    }
}
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using RichardSzalay.MockHttp;
using Microsoft.Extensions.DependencyInjection;
using Api.Shopify;
using System.Collections.Generic;
using System.Net.Mime;
using System.Net.Http.Json;
using Api.ApiLayer;
using System;
using FluentAssertions;
using System.Net;
using Api.BusinessLayer;
using Microsoft.EntityFrameworkCore;
using Api.DataLayer;

namespace Api.Tests
{
    public class ReplaceHttpClientComponentTest
    {
        private readonly WebApplicationFactory<Program> _webApplicationFactory;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly MockHttpMessageHandler MockHttpMessageHandler = new();

        public ReplaceHttpClientComponentTest(ITestOutputHelper testOutputHelper)
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
                });
        }

        [Fact]
        public async Task When_create_order_is_called_should_return_ok()
        {
            // Arrange
            MockHttpMessageHandler
                .When(HttpMethod.Get, "https://shopify/api/v1/products/admin/api/2021-07/products/APPLE_IPHONE.json")
                .WithHeaders(new Dictionary<string, string>
                {
                    ["X-Shopify-Access-Token"] = "secret"
                })
                .Respond(MediaTypeNames.Application.Json, File.ReadAllText("Shopify/Examples/APPLE_IPHONE.json"));

            // Act
            HttpResponseMessage response = await SystemUnderTest.PostAsync("/api/v1/orders", JsonContent.Create(new CreateOrderDto()
            {
                ProductNumbers = new [] { "APPLE_IPHONE" },
                UserId = Guid.NewGuid(),
                TotalAmount = 495m,
            }));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_create_order_is_called_should_return_bad_request()
        {
            // Arrange
            MockHttpMessageHandler
                .When(HttpMethod.Get, "https://shopify/api/v1/products/admin/api/2021-07/products/UNKNOWN.json")
                .Respond(HttpStatusCode.NotFound);

            // Act
            HttpResponseMessage response = await SystemUnderTest.PostAsync("/api/v1/orders", JsonContent.Create(new CreateOrderDto()
            {
                ProductNumbers = new[] { "UNKNOWN" },
                UserId = Guid.NewGuid(),
                TotalAmount = 495m,
            }));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            string message = await response.Content.ReadAsStringAsync();
            message.Should().Be("Unable to find product: UNKNOWN");
        }

        public HttpClient SystemUnderTest =>
            _webApplicationFactory.CreateClient();
    }
}
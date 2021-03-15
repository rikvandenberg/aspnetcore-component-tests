using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Api.Tests
{
    public class OrderApiComponentTest
    {
        private readonly WebApplicationFactory<Program> _webApplicationFactory;
        private readonly ITestOutputHelper _testOutputHelper;

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
                });
        }

        [Fact]
        public async Task When_application_starts_up_should_load_config_from_json_file()
        {
            // Arrange
            IConfiguration configuration = _webApplicationFactory.Services.GetRequiredService<IConfiguration>();

            // Act/Assert
            configuration["Shopify:ApiKey"].Should().Be("COMPONENT_TEST");
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
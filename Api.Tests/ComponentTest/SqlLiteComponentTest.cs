﻿using System;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Api.DataLayer;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Api.Tests
{
    public class SqlLiteComponentTest : IDisposable
    {
        private readonly WebApplicationFactory<Program> _webApplicationFactory;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly DbConnection _connection;

        public SqlLiteComponentTest(ITestOutputHelper testOutputHelper)
        {
            _webApplicationFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(CustomizeWebHostBuilder);
            _testOutputHelper = testOutputHelper;
            _connection = CreateInMemoryDatabase();
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
                    services.AddSingleton((serviceProvider) =>
                    {
                        var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>()
                            .UseSqlite(_connection);
                        return optionsBuilder.Options;
                    });
                });
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }


        [Fact]
        public async Task When_GET_is_called_on_the_api_should_return_200_OK()
        {
            // Arrange
            using IServiceScope scope = _webApplicationFactory.Services.CreateScope();
            Fixture fixture = new Fixture();
            Order order = fixture.Create<Order>();
            order.Id = Guid.NewGuid();
            IRepository<Order> repository = scope.ServiceProvider.GetRequiredService<IRepository<Order>>();
            await repository.CreateAsync(order);

            // Act
            HttpResponseMessage response = await SystemUnderTest.GetAsync($"/api/v1/orders/{order.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_GET_is_called_on_the_api_should_return_400_NotFound()
        {
            // Arrange

            // Act
            HttpResponseMessage response = await SystemUnderTest.GetAsync($"/api/v1/orders/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        public HttpClient SystemUnderTest =>
            _webApplicationFactory.CreateClient();
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Api.DataLayer;
using AutoFixture;
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
    public class InMemoryRepositoryComponentTest
    {
        private readonly WebApplicationFactory<Program> _webApplicationFactory;
        private readonly ITestOutputHelper _testOutputHelper;

        public InMemoryRepositoryComponentTest(ITestOutputHelper testOutputHelper)
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
                    services.AddSingleton(typeof(IRepository<>), typeof(InMemoryRepository<>));
                });
        }

        [Fact]
        public async Task When_GET_is_called_on_the_api_should_return_200_OK()
        {
            // Arrange
            Fixture fixture = new Fixture();
            Order order = fixture.Create<Order>();
            order.Id = Guid.NewGuid();
            IRepository<Order> repository = _webApplicationFactory.Services.GetRequiredService<IRepository<Order>>();
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

        public HttpClient SystemUnderTest =>
            _webApplicationFactory.CreateClient();
    }

    public sealed class InMemoryRepository<T> : IRepository<T>
        where T : class
    {
        private readonly IDictionary<string, T> _entities = new Dictionary<string, T>();

        public IQueryable<T> AllRecords => _entities.Values.AsQueryable();

        public Task CreateAsync(T entity)
        {
            dynamic newEntity = entity;
            _entities.Add(newEntity.Id.ToString(), entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            if(_entities.ContainsKey(id.ToString()))
            {
                return Task.CompletedTask;
            }

            _entities.Remove(id.ToString());
            return Task.CompletedTask;
        }

        public Task<T?> GetByIdAsync(string id)
        {
            return Task.FromResult(
                _entities.ContainsKey(id)
                ? _entities[id]
                : null);
        }

        public Task UpdateAsync(T entity)
        {
            // ReferenceType, don't need to do anything;
            return Task.CompletedTask;
        }
    }
}
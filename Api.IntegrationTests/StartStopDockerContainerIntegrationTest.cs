using Docker.DotNet;
using Docker.DotNet.Models;
using Xunit.Abstractions;
using FluentAssertions;
using Microsoft.VisualBasic;
using DotNet.Testcontainers.Builders;

namespace Api.IntegrationTests;

public class StartStopDockerContainerIntegrationTest : IDisposable
{
    private readonly ITestOutputHelper _testOutputHelper;

    public DockerClient Docker { get; }

    public StartStopDockerContainerIntegrationTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        Docker = new DockerClientConfiguration()
             .CreateClient();
    }

    [Fact]
    public async Task Should_pull_start_and_kill_container()
    {
        // When
        Progress<JSONMessage> progress = new Progress<JSONMessage>();
        await Docker.Images.CreateImageAsync(new ImagesCreateParameters
        {
            FromImage = "nginxdemos/hello",
            Tag = "latest",
        }, null, progress);

        var container = await Docker.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = "nginxdemos/hello",
        });
        bool result = await Docker.Containers.StartContainerAsync(container.ID, new());

        // Then
        result.Should().BeTrue();

        await Docker.Containers.KillContainerAsync(container.ID, new());
        await Docker.Containers.RemoveContainerAsync(container.ID, new());
    }

    [Fact]
    public async Task Should_pull_start_and_kill_container_with_testcontainers()
    {
        // When
        var container = new ContainerBuilder()
            .WithImage("nginxdemos/hello:latest")
            .Build();

        await container.StartAsync();

        // Then
        await container.StopAsync();
        await container.DisposeAsync();
    }

    public void Dispose()
    {
        Docker.Dispose();
    }
}


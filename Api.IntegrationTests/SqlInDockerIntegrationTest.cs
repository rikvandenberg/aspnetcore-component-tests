using Docker.DotNet;
using Docker.DotNet.Models;
using Xunit.Abstractions;
using FluentAssertions;

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
    public async Task When_docker_mysql_is_started_should_insert_record()
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

    public void Dispose()
    {
        Docker.Dispose();
    }
}


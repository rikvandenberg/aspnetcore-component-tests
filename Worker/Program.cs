
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Threading.Channels;
using Worker;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

// RabbitMQ
builder.Services.AddOptionsWithValidation<RabbitMqSettings>("RabbitMq");
builder.Services.AddSingleton<ConnectionFactory>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<RabbitMqSettings>>();
    return new ConnectionFactory
    {
        HostName = settings.Value.HostName,
        Password = settings.Value.Password,
        UserName = settings.Value.UserName
    };
});
builder.Services.AddSingleton<IConnection>(sp => sp.GetRequiredService<ConnectionFactory>().CreateConnection());
builder.Services.AddTransient<IModel>(sp => sp.GetRequiredService<IConnection>().CreateModel());

builder.Services.AddHostedService<WorkerBackgroundService>();
WebApplication app = builder.Build();
await InitAsync(app.Services);
await app.RunAsync();


static async Task InitAsync(IServiceProvider serviceProvider)
{
    await SetupRedisExchangeAsync(serviceProvider.GetRequiredService<IModel>(), serviceProvider.GetRequiredService<ILogger<Program>>());
}

static async Task SetupRedisExchangeAsync(IModel channel, ILogger<Program> logger)
{
    channel.ExchangeDeclare(RabbitMqHelper.ExchangeNames.Orders, type: ExchangeType.Topic, durable: true);
    logger.LogDebug("Exchange created");
    channel.QueueDeclare(RabbitMqHelper.QueueNames.Worker, durable: true, exclusive: false, autoDelete: false);
    logger.LogDebug("Queue created");
    channel.QueueBind(RabbitMqHelper.QueueNames.Worker, RabbitMqHelper.ExchangeNames.Orders, "#");
    logger.LogDebug("Queue binding created");
}

public static class RabbitMqHelper
{
    public static class ExchangeNames
    {
        public static readonly string Orders = "orders";
    }

    public static class QueueNames
    {
        public static readonly string Worker = "worker";
    }
}

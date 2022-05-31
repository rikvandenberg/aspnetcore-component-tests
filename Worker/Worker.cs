using System.Diagnostics;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Worker;

public class WorkerBackgroundService : BackgroundService
{
    private readonly ILogger<WorkerBackgroundService> _logger;
    private readonly IModel _channel;

    public WorkerBackgroundService(IModel channel, ILogger<WorkerBackgroundService> logger)
    {
        _channel = channel;
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("Backgroundworker started");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("Backgroundworker stopped");
        return base.StopAsync(cancellationToken);
    }


    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);
        consumer.Received += OnConsumerReceived;
        _channel.BasicConsume(RabbitMqHelper.QueueNames.Worker, autoAck: false, consumer);
        return Task.CompletedTask;
    }

    private void OnConsumerReceived(object? sender, BasicDeliverEventArgs eventArgs)
    {
        OnConsumerReceived(sender as EventingBasicConsumer, eventArgs);
    }

    private void OnConsumerReceived(EventingBasicConsumer consumer, BasicDeliverEventArgs eventArgs)
    {
        byte[] body = eventArgs.Body.ToArray();
        string message = Encoding.UTF8.GetString(body);
        string routingKey = eventArgs.RoutingKey;

        _logger.LogDebug(" [x] Received '{0}':'{1}'", routingKey, message);
        _logger.LogDebug(" [x] Done");

        IModel channel = consumer.Model;
        channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);
    }
}

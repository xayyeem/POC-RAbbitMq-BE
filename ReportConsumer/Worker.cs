using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReportConsumer.Data;
using ReportConsumer.Services;
using System.Text;
using System.Text.Json;

namespace ReportConsumer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly SyncRepository _syncRepository;
        private readonly ReportRepository _reportRepository;
        private readonly ExcelReportGenerator _excelReportGenerator;

        public Worker(
    IConfiguration configuration,
    SyncRepository syncRepository,
    ReportRepository reportRepository,
    ExcelReportGenerator excelReportGenerator)
        {
            _configuration = configuration;
            _syncRepository = syncRepository;
            _reportRepository = reportRepository;
            _excelReportGenerator = excelReportGenerator;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var host = _configuration["RabbitMQ:Host"];
            var port = int.Parse(_configuration["RabbitMQ:Port"]!);
            var username = _configuration["RabbitMQ:UserName"];
            var password = _configuration["RabbitMQ:Password"];
            var queueName = _configuration["RabbitMQ:QueueName"];
            
            var factory = new ConnectionFactory
            {
                HostName = host,
                Port = port,
                UserName = username,
                Password = password
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            Console.WriteLine("RabbitMQ Consumer Started...");
            Console.WriteLine($"Listening on queue: {queueName}");

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();

                    var json = Encoding.UTF8.GetString(body);

                    Console.WriteLine("=================================");
                    Console.WriteLine("Message Received");
                    Console.WriteLine(json);
                    Console.WriteLine("=================================");

                    // Check sync status
                    var isSynced =
                        await _syncRepository.IsDataSyncedAsync();

                    Console.WriteLine(
                        $"Sync Status: {isSynced}");

                    if (!isSynced)
                    {
                        Console.WriteLine(
                            "Data is not synced. Report will remain in queue.");

                        await Task.Delay(
                            TimeSpan.FromSeconds(5),
                            stoppingToken);

                        await _channel!.BasicNackAsync(
                            deliveryTag: ea.DeliveryTag,
                            multiple: false,
                            requeue: true);

                        return;
                    }

                    // Deserialize RabbitMQ message
                    var message =
                        JsonSerializer.Deserialize<ReportQueueMessage>(
                            json);

                    if (message == null)
                    {
                        Console.WriteLine(
                            "Unable to deserialize report message.");

                        await _channel!.BasicNackAsync(
                            deliveryTag: ea.DeliveryTag,
                            multiple: false,
                            requeue: false);

                        return;
                    }

                    var requestId =
                        message.RequestId;

                    var reportName =
                        message.ReportRequest?.Report?.ReportNames
                        ?? "Report";

                    Console.WriteLine(
                        $"RequestId: {requestId}");

                    Console.WriteLine(
                        $"ReportName: {reportName}");

                    // Update DB -> Processing
                    await _reportRepository.UpdateProcessingAsync(
                        requestId);

                    Console.WriteLine(
                        "Status updated to Processing.");

                    // Generate Excel
                    var filePath =
                        _excelReportGenerator.Generate(
                            requestId,
                            reportName);

                    Console.WriteLine(
                        $"Excel generated: {filePath}");

                    // Update DB -> Completed
                    await _reportRepository.UpdateCompletedAsync(
                        requestId,
                        filePath);

                    Console.WriteLine(
                        "Status updated to Completed.");

                    // ACK RabbitMQ message
                    await _channel!.BasicAckAsync(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false);

                    Console.WriteLine(
                        "Message acknowledged.");

                    Console.WriteLine("=================================");
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine(
                        "Worker stopping...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Error processing message: {ex}");

                    try
                    {
                        await _channel!.BasicNackAsync(
                            deliveryTag: ea.DeliveryTag,
                            multiple: false,
                            requeue: true);
                    }
                    catch
                    {
                        // Channel may already be closed.
                    }
                }
            };

            await _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel != null)
                await _channel.CloseAsync();

            if (_connection != null)
                await _connection.CloseAsync();

            await base.StopAsync(cancellationToken);
        }
    }
}

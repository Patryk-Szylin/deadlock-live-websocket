
using Confluent.Kafka;

public class KafkaBackgroundService : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    
    public KafkaBackgroundService(IConfiguration configuration)
    {
        var bootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers") ?? "localhost:9092";

        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "websocket-consumers",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("KafkaBackgroundService started.");
        
        // Subscribe to all events in the group
        _consumer.Subscribe("^game-streams-*");
        
        return Task.Run(() =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(stoppingToken);
                Console.WriteLine($"Received message: {result.Message.Value}");
            }
        }, stoppingToken);
    }
}

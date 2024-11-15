using System.Collections.Concurrent;
using Confluent.Kafka;

public class KafkaConsumerService
{
    private readonly IConsumer<string, string> _consumer;

    public KafkaConsumerService(IConfiguration configuration)
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

    public async Task SubscribeToMatchAsync(string matchId, Func<string, Task> onMessageReceived)
    {
        var topic = $"game-streams-{matchId}";
        _consumer.Subscribe(topic);

        await Task.Run(() =>
        {
            while (true)
            {
                var result = _consumer.Consume();
                onMessageReceived?.Invoke(result.Message.Value);
            }
        });
    }

}

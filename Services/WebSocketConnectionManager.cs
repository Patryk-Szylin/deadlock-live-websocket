
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;

public class WebSocketConnectionManager
{
    private readonly ConcurrentDictionary<string, List<WebSocket>> _connections = new();
    private readonly KafkaConsumerService _kafkaConsumer;

    public WebSocketConnectionManager(KafkaConsumerService kafkaConsumer)
    {
        _kafkaConsumer = kafkaConsumer;
    }
    public void AddConnection(string matchId, WebSocket socket)
    {
        if (!_connections.ContainsKey(matchId))
        {
            _connections[matchId] = new List<WebSocket>();
        }

        _connections[matchId].Add(socket);
    }

    public void RemoveConnection(string matchId, WebSocket socket)
    {
        if (_connections.ContainsKey(matchId))
        {
            _connections[matchId].Remove(socket);

            if (!_connections[matchId].Any())
            {
                _connections.TryRemove(matchId, out _);
            }
        }
    }

    public async Task BroadcastMessageAsync(string matchId, string message)
    {
        // Extract matchId from topic name

        if (_connections.TryGetValue(matchId, out var sockets))
        {
            var tasks = sockets
                .Select(socket => SendMessageAsync(socket, message));

            await Task.WhenAll(tasks); // Broadcast message to all active WebSocket clients
        }
    }
    private static async Task SendMessageAsync(WebSocket socket, string message)
    {
        var buffer = System.Text.Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(buffer);

        await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
    }
    public async Task HandleWebSocketConnectionAsync(WebSocket socket, KafkaConsumerService kafkaConsumer)
    {
        var buffer = new byte[1024 * 4];
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        var json = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
        var matchId = JsonSerializer.Deserialize<MatchSubscriptionRequest>(json)?.MatchId;

        if (string.IsNullOrEmpty(matchId))
        {
            throw new InvalidOperationException("Invalid WebSocket message: matchId is required.");
        }

        AddConnection(matchId, socket);
        
        // Subscribe to Kafka topic
        await kafkaConsumer.SubscribeToMatchAsync(matchId, async message =>
        {
            await BroadcastMessageAsync(matchId, message);
        });

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                await Task.Delay(1000); // Keep connection alive
            }
        }
        finally
        {
            RemoveConnection(matchId, socket);
        }
    }
}

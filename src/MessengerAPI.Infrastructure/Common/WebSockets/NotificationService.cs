using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MessengerAPI.Application.Common.Interfaces;
using StackExchange.Redis;

namespace MessengerAPI.Infrastructure.Common.WebSockets;

public class NotificationService : INotificationService, IWebSocketService
{
    private static readonly ConcurrentDictionary<Guid, WebSocket> _connections = new();
    private readonly ConnectionRepository _connectionRepository;
    private readonly string _serverId;
    private readonly IDatabase _redis;

    public NotificationService(
        ConnectionRepository connectionRepository,
        IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionRepository = connectionRepository;
        _serverId = Environment.MachineName;
        _redis = connectionMultiplexer.GetDatabase();
    }

    public async Task AddConnectionAsync(Guid userId, WebSocket webSocket)
    {
        _connections[userId] = webSocket;
        await _connectionRepository.AddAsync(userId, _serverId);
    }

    public async Task RemoveConnectionAsync(Guid userId)
    {
        if (_connections.ContainsKey(userId))
        {
            _connections.Remove(userId, out _);
            await _connectionRepository.RemoveAsync(userId);
        }
    }

    public async Task NotifyAsync(List<Guid> recipientIds, string jsonData)
    {
        var groups = new Dictionary<string, List<Guid>>();
        var currentServer = new HashSet<Guid>();

        foreach (var userId in recipientIds)
        {
            if (_connections.ContainsKey(userId))
            {
                currentServer.Add(userId);
            }
            else
            {
                var serverId = await _connectionRepository.GetAsync(userId);
                if (serverId != null)
                {
                    if (!groups.ContainsKey(serverId))
                    {
                        groups[serverId] = new List<Guid>();
                    }
                    groups[serverId].Add(userId);
                }
            }
        }

        foreach (string pipe in groups.Keys)
        {
            var notificationMessage = new NotificationMessage(groups[pipe], jsonData);
            string jsonMessage = JsonSerializer.Serialize(notificationMessage);
            await _redis.PublishAsync(pipe, jsonMessage);
        }
        foreach (var recipientId in currentServer)
        {
            await SendMessage(recipientId, jsonData);
        }
    }

    private async Task SendMessage(Guid recipientId, string jsonMessage)
    {
        if (_connections.ContainsKey(recipientId))
        {
            await SendMessageToWebSocket(recipientId, jsonMessage);
        }
        else
        {
            string? serverId = await _connectionRepository.GetAsync(recipientId);
            // TODO: check that queue exists
            if (serverId != null)
            {
                await _redis.PublishAsync(serverId, jsonMessage);
            }
        }
    }

    private async Task SendMessageToWebSocket(Guid userId, string jsonMessage)
    {
        if (_connections.TryGetValue(userId, out var webSocket))
        {
            await webSocket.SendAsync(
                new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonMessage)),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }
    }
}

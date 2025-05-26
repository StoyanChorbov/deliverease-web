using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Application.Hubs;

// Hub for real-time location sharing
public class LocationsHub : Hub
{
    // All current connections
    // Key: Username, Value: ConnectionId
    private static readonly ConcurrentDictionary<string, string> Connections = new();
    
    // Connect to the hub
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("New connection");
        var username = Context.User?.Identity?.Name;
        if (username != null)
        {
            Connections[username] = Context.ConnectionId;
            Console.WriteLine("Connected user: " + username);
        }
        await base.OnConnectedAsync();
    }
    
    // Disconnect from the hub
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine("Disconnected connection");
        var username = Context.User?.Identity?.Name;
        if (username != null)
        {
            Connections.TryRemove(username, out _);
        }
        await base.OnDisconnectedAsync(exception);
    }
    
    // Request location from deliverer
    public async Task RequestDelivererLocation(string delivererUsername)
    {
        if (Connections.TryGetValue(delivererUsername, out var connectionId))
        {
            // Send request with username of the location recipient
            await Clients.Client(connectionId).SendAsync("RequestLocationUpdate", Context.User?.Identity?.Name);
        }
        else
        {
            await Clients.Caller.SendAsync("DelivererNotConnected", delivererUsername);
        }
    }
    
    // Send the location update to the recipient
    public async Task ReceiveLocationUpdate(string recipient, double latitude, double longitude)
    {
        if (Connections.TryGetValue(recipient, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("UpdateDelivererLocation", latitude, longitude);
        }
    }
}
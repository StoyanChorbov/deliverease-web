using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Application.Hubs;

// Hub for real-time location sharing
public class LocationsHub : Hub
{
    // All current connections
    private static readonly ConcurrentDictionary<string, string> Connections = new();
    
    // Connect to the hub
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("New connection");
        // var username = Context.User?.Identity?.Name;
        // if (username != null)
        // {
        //     Connections[Context.ConnectionId] = username;
        //     Console.WriteLine("Connected user: " + username);
        // }
        await base.OnConnectedAsync();
    }
    
    // Disconnect from the hub
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine("Disconnected connection");
        // var username = Context.User?.Identity?.Name;
        // if (username != null)
        // {
        //     Connections.TryRemove(Context.ConnectionId, out _);
        // }
        // Console.WriteLine("Disconnected user: " + username);
        await base.OnDisconnectedAsync(exception);
    }
    
    // Get location from deliverer
    // public async Task GetDelivererLocation(string delivererUsername, double latitude, double longitude)
    // {
    //     Console.WriteLine("Deliverer location update: " + delivererUsername + " - " + latitude + ", " + longitude);
    //     if (Connections.TryGetValue(delivererUsername, out var connectionId))
    //     {
    //         await Clients.Client(connectionId).SendAsync("ReceiveLocationUpdate", Context.User?.Identity?.Name, latitude, longitude);
    //     }
    // }

    // Send location to user
    // public async Task RespondWithLocation(string requestedConnectionId, string latitude, string longitude)
    // {
    //     await Clients.Client(requestedConnectionId).SendAsync("ReceiveLocationUpdate", Context.User?.Identity?.Name, latitude, longitude);
    // }
}
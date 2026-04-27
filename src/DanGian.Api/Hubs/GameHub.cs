using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DanGian.Api.Hubs;

[Authorize]
public sealed class GameHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"room:{roomId}");
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room:{roomId}");
    }

    public async Task JoinGame(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"game:{sessionId}");
    }

    public async Task LeaveGame(string sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"game:{sessionId}");
    }
}

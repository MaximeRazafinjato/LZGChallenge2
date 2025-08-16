using Microsoft.AspNetCore.SignalR;

namespace LZGChallenge2.Api.Hubs;

public class LeaderboardHub : Hub
{
    public async Task JoinLeaderboardGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Leaderboard");
    }
    
    public async Task LeaveLeaderboardGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Leaderboard");
    }
    
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Leaderboard");
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Leaderboard");
        await base.OnDisconnectedAsync(exception);
    }
}
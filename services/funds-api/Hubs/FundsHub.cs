using Microsoft.AspNetCore.SignalR;

namespace FundsApi.Hubs
{
    public class FundsHub : Hub
    {
        public async Task JoinWalletGroup(string accountNumber)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, accountNumber);
        }

        public async Task LeaveWalletGroup(string accountNumber)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, accountNumber);
        }
    }
}
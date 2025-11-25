using Microsoft.AspNetCore.SignalR;

namespace GastroArt.Hubs
{
    public class OrderHub : Hub
    {
        public async Task CallWaiter(string tableNo)
        {
            await Clients.All.SendAsync("ReceiveWaiterCall", tableNo);
        }
    }
}
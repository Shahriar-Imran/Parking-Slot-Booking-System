using Microsoft.AspNetCore.SignalR;

public class SlotHub : Hub
{
    public async Task BroadcastSlotUpdate(object data)
    {
        await Clients.All.SendAsync("ReceiveSlotUpdate", data);
    }
}
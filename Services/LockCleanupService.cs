using Microsoft.AspNetCore.SignalR;
using ParkingSystem.Data;

public class LockCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public LockCleanupService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var hub = scope.ServiceProvider.GetRequiredService<IHubContext<SlotHub>>();

            var expiredLocks = context.SlotLocks
                .Where(l => l.ExpireTime < DateTime.Now)
                .ToList();

            context.SlotLocks.RemoveRange(expiredLocks);
            await context.SaveChangesAsync();

            foreach (var l in expiredLocks)
            {
                await hub.Clients.All.SendAsync("ReceiveSlotUpdate", new
                {
                    slotId = l.SlotId,
                    status = "available",
                    userId = l.UserId
                });
            }

            await Task.Delay(1000); // every 2 sec
        }
    }
}
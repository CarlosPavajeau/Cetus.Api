using Microsoft.AspNetCore.SignalR;

namespace Cetus.Api.Realtime;

public interface IOrdersClient
{
    Task ReceiveCreatedOrder();
    Task ReceiveUpdatedOrder();
}

public sealed class OrdersHub : Hub<IOrdersClient>
{
    public async Task JoinOrderGroup(Guid orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, orderId.ToString());
    }
}

using Application.Orders;
using Microsoft.AspNetCore.SignalR;

namespace Cetus.Api.Realtime;

public interface IOrdersClient
{
    Task ReceiveCreatedOrder(SimpleOrderResponse order);
    Task ReceiveUpdatedOrder();
}

public sealed class OrdersHub : Hub<IOrdersClient>
{
    public async Task JoinOrderGroup(Guid orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, orderId.ToString());
    }

    public async Task JoinStoreGroup(string storeSlug)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, storeSlug);
    }
}

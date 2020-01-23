using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
//using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace WebAPI.NetCore.Core
{
    //[Microsoft.AspNet.SignalR.Hubs.HubName("clientHub")]
    [EnableCors("CorsPolicy")]
    public class ClientHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
    // You can inject the IHubContext<T> class into a service and call the clients using that.
    //public class NotifyService
    //{
    //    private readonly IHubContext<ClientHub> _hub;
    //    public NotifyService(IHubContext<ClientHub> hub)
    //    {
    //        _hub = hub;
    //    }
    //    public Task SendNotificationAsync(string message)
    //    {
    //        return _hub.Clients.All.SendAsync("ReceiveMessage", message);
    //    }
    //}

}

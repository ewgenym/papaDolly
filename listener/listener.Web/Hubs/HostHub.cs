using Microsoft.AspNet.SignalR;

namespace listener.Web.Hubs
{
    public class HostHub : Hub
    {
        public void Send(int value)
        {
            Clients.All.hostValue(value);
        }
    }
}
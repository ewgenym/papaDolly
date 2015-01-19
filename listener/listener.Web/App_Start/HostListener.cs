using System;
using System.Threading;
using System.Threading.Tasks;
using listener.Web.App_Start;
using listener.Web.Hubs;
using Microsoft.AspNet.SignalR;
using NetMQ;
using NetMQ.Sockets;
using WebActivatorEx;

[assembly: PostApplicationStartMethod(typeof(HostListener), "Start")]
[assembly: ApplicationShutdownMethod(typeof(HostListener), "Shutdown")]

namespace listener.Web.App_Start
{
    public static class HostListener
    {
        private static NetMQContext _context;
        private static SubscriberSocket _subscriber;
        private static CancellationTokenSource _cts;
        private static Task _readerTask;

        public static void Start()
        {
            _cts = new CancellationTokenSource();
            _readerTask = Task.Factory.StartNew(obj =>
            {
                var token = (CancellationToken) obj;
                _context = NetMQContext.Create();

                _subscriber = _context.CreateSubscriberSocket();

                _subscriber.Connect("tcp://127.0.0.1:5555");
                _subscriber.Subscribe("");

                while (!token.IsCancellationRequested)
                {
                    if (!_subscriber.HasIn)
                    {
                        continue;
                    }

                    bool more;
                    var bytes = _subscriber.Receive(out more);

                    var clients = GlobalHost.ConnectionManager.GetHubContext<HostHub>().Clients;
                    clients.All.HostValue(bytes[0]);
                }

                throw new OperationCanceledException(token);
            }, _cts.Token);
        }

        public static void Shutdown()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _readerTask.Wait(_cts.Token);
                _cts.Dispose();
            }
            
            if (_subscriber != null)
            {
                _subscriber.Dispose();
            }

            if (_context != null)
            {
                _context.Dispose();
            }
        }
    }
}
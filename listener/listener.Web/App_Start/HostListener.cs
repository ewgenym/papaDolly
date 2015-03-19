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
        private static SubscriberSocket _eventSubscriber;
        private static SubscriberSocket _statusSubscriber;
        private static CancellationTokenSource _cts;
        private static Task _readerTask;

        public static void Start()
        {
            _cts = new CancellationTokenSource();
            _context = NetMQContext.Create();

            _readerTask = Task.Factory.StartNew(obj =>
            {
                var token = (CancellationToken) obj;

                _eventSubscriber = _context.CreateSubscriberSocket();
                _eventSubscriber.Connect("tcp://127.0.0.1:5555");
                _eventSubscriber.Subscribe("");

                _statusSubscriber = _context.CreateSubscriberSocket();
                _statusSubscriber.Connect("tcp://127.0.0.1:5556");
                _statusSubscriber.Subscribe("");

                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    if (_eventSubscriber.HasIn)
                    {
                        bool more;
                        var bytes = _eventSubscriber.Receive(out more);

                        var clients = GlobalHost.ConnectionManager.GetHubContext<HostHub>().Clients;
                        clients.All.HostValue(bytes[0]);
                    }

                    if (_statusSubscriber.HasIn)
                    {
                        var status = _statusSubscriber.ReceiveString();
                        var clients = GlobalHost.ConnectionManager.GetHubContext<HostHub>().Clients;
                        clients.All.HostStatus(status);
                    }
                }
            }, _cts.Token, TaskCreationOptions.LongRunning);
        }

        public static void Shutdown()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                try
                {
                    _readerTask.Wait(_cts.Token);
                }
                catch (AggregateException)
                {
                }
                _cts.Dispose();
            }

            if (_statusSubscriber != null)
            {
                _statusSubscriber.Dispose();
            }

            if (_eventSubscriber != null)
            {
                _eventSubscriber.Dispose();
            }

            if (_context != null)
            {
                _context.Dispose();
            }
        }
    }
}
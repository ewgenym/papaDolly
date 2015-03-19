using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using listener.Common;
using listener.Common.Infrastructure;
using NetMQ;
using NetMQ.Sockets;

namespace listener.Bg
{
    public partial class HostReaderService : ServiceBase
    {
        private EventLog _eventLog;
        private TableHostPacketReader _reader;
        private NetMQContext _context;
        private PublisherSocket _eventPublisher;
        private PublisherSocket _statusPublisher;
        private CancellationTokenSource _statusPublisherCancellation;
        private Task _statusPublisherTask;
        private TableStatusResolver _statusResolver;

        public HostReaderService()
        {
            InitializeComponent();

            InitLog();
        }

        private void InitMq()
        {
            _context = NetMQContext.Create();

            _eventPublisher = _context.CreatePublisherSocket();
            _eventPublisher.Bind("tcp://*:5555");

            _statusPublisher = _context.CreatePublisherSocket();
            _statusPublisher.Bind("tcp://*:5556");
        }

        private void RunEventReaderTask()
        {
            _reader = new TableHostPacketReader();
            _reader.PacketReceived += PacketReceivedHandler;

            _reader.Start();
        }

        private void RunStatusPublisherTask()
        {
            _statusPublisherCancellation = new CancellationTokenSource();
            _statusPublisherTask = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    _statusPublisher.Send(_statusResolver.Status);
                    await Task.Delay(1000, _statusPublisherCancellation.Token);
                }
            }, _statusPublisherCancellation.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        private void StopStatusPublisherTask()
        {
            if (_statusPublisherCancellation != null)
            {
                _statusPublisherCancellation.Cancel();
            }

            if (_statusPublisherTask != null)
            {
                try
                {
                    _statusPublisherTask.Wait();
                }
                catch (AggregateException)
                {
                }
            }
        }

        private void PacketReceivedHandler(object sender, TablePacket e)
        {
            _eventPublisher.Send(new byte[] {100});
            _statusResolver.Tick();
            _eventLog.WriteEntry(string.Format("0x{0:X}", e.Id));
        }

        private void InitLog()
        {
            _eventLog = new EventLog();
            if (!EventLog.SourceExists("HostReaderSource"))
            {
                EventLog.CreateEventSource("HostReaderSource", "HostReaderLog");
            }

            _eventLog.Source = "HostReaderSource";
            _eventLog.Log = "HostReaderLog";
        }

        protected override void OnStart(string[] args)
        {
            InitMq();

            RunEventReaderTask();

            _statusResolver = new TableStatusResolver();

            RunStatusPublisherTask();

            _eventLog.WriteEntry("Started listening.");
        }

        protected override void OnStop()
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            StopStatusPublisherTask();

            if (_statusResolver != null)
            {
                _statusResolver.Dispose();
            }

            if (_statusPublisher != null)
            {
                _statusPublisher.Dispose();
            }

            if (_eventPublisher != null)
            {
                _eventPublisher.Dispose();
            }

            if (_context != null)
            {
                _context.Dispose();
            }

            _eventLog.WriteEntry("Stoped listening.");
        }
    }
}

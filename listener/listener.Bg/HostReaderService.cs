using System.Diagnostics;
using System.ServiceProcess;
using System.Threading.Tasks;
using listener.Common.Infrastructure;
using NetMQ;
using NetMQ.Sockets;

namespace listener.Bg
{
    public partial class HostReaderService : ServiceBase
    {
        private EventLog _eventLog;
        private Task _readerTask;
        private TableHostPacketReader _reader;
        private NetMQContext _context;
        private PublisherSocket _publisher;

        public HostReaderService()
        {
            InitializeComponent();

            InitLog();
        }

        private void InitMq()
        {
            _context = NetMQContext.Create();
            _publisher = _context.CreatePublisherSocket();
            _publisher.Bind("tcp://*:5555");
        }

        private void RunReaderTask()
        {
            _reader = new TableHostPacketReader();
            _reader.PacketReceived += PacketReceivedHandler;

            _reader.Start();
        }

        private void PacketReceivedHandler(object sender, TablePacket e)
        {
            _publisher.Send(new []{e.Adch, e.Adcl});
            _eventLog.WriteEntry(string.Format("0x{0:X}; 0x{1:X}", e.Adch, e.Adcl));
        }

        private void InitLog()
        {
            _eventLog = new EventLog();
            if (!EventLog.SourceExists("HostReaderSource"))
            {
                EventLog.CreateEventSource(
                    "HostReaderSource", "HostReaderLog");
            }
            _eventLog.Source = "HostReaderSource";
            _eventLog.Log = "HostReaderLog";
        }

        protected override void OnStart(string[] args)
        {
            InitMq();

            RunReaderTask();

            _eventLog.WriteEntry("Started listening.");
        }

        protected override void OnStop()
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_publisher != null)
            {
                _publisher.Dispose();
            }

            if (_context != null)
            {
                _context.Dispose();
            }

            _eventLog.WriteEntry("Stoped listening.");
        }
    }
}

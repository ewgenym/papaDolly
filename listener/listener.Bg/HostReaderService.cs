using System.ServiceProcess;
using listener.Common.Infrastructure;
using NetMQ;
using NetMQ.WebSockets;

namespace listener.Bg
{
    public partial class HostReaderService : ServiceBase
    {
        private readonly string _endpoint;
        private TableHostPacketReader _reader;
        private NetMQContext _context;
        private WSPublisher _eventPublisher;

        public HostReaderService(string endpoint)
        {
            _endpoint = endpoint;
            InitializeComponent();
        }

        private void InitMq()
        {
            _context = NetMQContext.Create();

            _eventPublisher = _context.CreateWSPublisher();
            _eventPublisher.Bind(_endpoint);
        }

        private void RunEventReaderTask()
        {
            _reader = new TableHostPacketReader();
            _reader.PacketReceived += PacketReceivedHandler;

            _reader.Start();
        }

        private void PacketReceivedHandler(object sender, TablePacket e)
        {
            _eventPublisher.Send("event");
        }

        protected override void OnStart(string[] args)
        {
            InitMq();

            RunEventReaderTask();
        }

        protected override void OnStop()
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_eventPublisher != null)
            {
                _eventPublisher.Dispose();
            }

            if (_context != null)
            {
                _context.Dispose();
            }
        }
    }
}

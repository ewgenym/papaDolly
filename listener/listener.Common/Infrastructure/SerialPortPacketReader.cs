using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace listener.Common.Infrastructure
{
    public abstract class SerialPortPacketReader<TPacket> : IDisposable
        where TPacket: class
    {
        private readonly SerialPort _port;
        private readonly Queue<byte> _queue;

        protected SerialPortPacketReader(string portName, int baudRate)
        {
            _port = new SerialPort(portName)
            {
                BaudRate = baudRate,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8,
                Handshake = Handshake.None,
            };
            _port.DataReceived += DataReceived;
            _queue = new Queue<byte>();
        }

        public event EventHandler<TPacket> PacketReceived;

        protected virtual void OnPacketReceived(TPacket e)
        {
            var handler = PacketReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var sp = (SerialPort)sender;

            var buffer = new byte[sp.BytesToRead];
            sp.Read(buffer, 0, sp.BytesToRead);
            Array.ForEach(buffer, b => _queue.Enqueue(b));
            var packet = CheckForPacketReceived(_queue);
            if (packet != null)
            {
                OnPacketReceived(packet);
            }
        }

        protected abstract TPacket CheckForPacketReceived(Queue<byte> queue);

        public void Start()
        {
            _port.Open();
        }

        public void Dispose()
        {
            _port.Close();
        }
    }
}
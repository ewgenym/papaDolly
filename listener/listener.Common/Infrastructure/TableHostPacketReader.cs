using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;

namespace listener.Common.Infrastructure
{
    public class TableHostPacketReader : SerialPortPacketReader<TablePacket>
    {
        const int PacketSize = 3;

        public TableHostPacketReader()
            : base(ConfigurationManager.AppSettings["port"], 1200)
        {
        }

        protected override TablePacket CheckForPacketReceived(Queue<byte> queue)
        {
            const byte packetStart = 0xFF;
            const byte packetStop = 0xFA;

            while (queue.Count > 0 && queue.Peek() != packetStart)
            {
                queue.Dequeue();
            }

            if (queue.Count < PacketSize)
            {
                return default(TablePacket);
            }

            var start = queue.Dequeue();
            Debug.Assert(start == packetStart);

            var id = queue.Dequeue();

            var stop = queue.Dequeue();

            Debug.Assert(stop == packetStop);

            return new TablePacket(id);
        }
    }
}
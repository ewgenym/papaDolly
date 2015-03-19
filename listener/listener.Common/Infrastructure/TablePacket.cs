namespace listener.Common.Infrastructure
{
    public class TablePacket
    {
        public byte Id { get; private set; }

        public TablePacket(byte id)
        {
            Id = id;
        }
    }
}
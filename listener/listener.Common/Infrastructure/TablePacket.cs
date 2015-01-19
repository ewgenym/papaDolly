namespace listener.Common.Infrastructure
{
    public class TablePacket
    {
        public byte Id { get; private set; }
        public byte Adcl { get; private set; }
        public byte Adch { get; private set; }

        public TablePacket(byte id, byte adcl, byte adch)
        {
            Id = id;
            Adcl = adcl;
            Adch = adch;
        }
    }
}
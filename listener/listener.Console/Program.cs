namespace listener.Console
{
    class Program
    {
        private static void Main(string[] args)
        {
            using (var reader = new TableHostPacketReader())
            {
                reader.PacketReceived += PacketReceivedHandler;

                reader.Start();

                System.Console.WriteLine("Press any key to continue...");
                System.Console.WriteLine();
                System.Console.ReadKey();
            }
        }

        private static void PacketReceivedHandler(object sender, TablePacket e)
        {
            System.Console.WriteLine("{0}; {1}", e.Adch, e.Adcl);
        }
    }
}

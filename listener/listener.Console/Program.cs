using listener.Common.Infrastructure;
using NetMQ;

namespace listener.Console
{
    class Program
    {
        private static void Main(string[] args)
        {
            ReadingHost();
            //ListeningQueue();
        }

        private static void ListeningQueue()
        {
            using (NetMQContext context = NetMQContext.Create())
            {
                using (var subscriber = context.CreateSubscriberSocket())
                {
                    subscriber.Connect("tcp://127.0.0.1:5555");

                    subscriber.Subscribe("");

                    while (true)
                    {
                        bool more;
                        var bytes = subscriber.Receive(out more);

                        System.Console.WriteLine("0x{0:X}", bytes[0]);
                    }
                }
            }
        }

        private static void ReadingHost()
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
            System.Console.WriteLine("0x{0:X}", e.Id);
        }
    }
}

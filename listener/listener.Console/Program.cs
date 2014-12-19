using System;
using System.IO;
using System.IO.Ports;

namespace listener.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var serialPort = new SerialPort("COM9")
                {
                    BaudRate = 1200,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    DataBits = 8,
                    Handshake = Handshake.None,
                })
            {
                serialPort.DataReceived += DataReceivedHandler;
                //serialPort.ErrorReceived += ErrorReceivedHandler;
                //serialPort.PinChanged += PinChangedHandler;

                serialPort.Open();



               //read(serialPort);


                System.Console.WriteLine("Press any key to continue...");
                System.Console.WriteLine();
                System.Console.ReadKey();
            }
        }

        private static void read(SerialPort serialPort)
        {
            int blockLimit = 3;
            byte[] buffer = new byte[blockLimit];

            serialPort.BaseStream.BeginRead(buffer, 0, buffer.Length, delegate(IAsyncResult ar)
            {
                try
                {
                    int actualLength = serialPort.BaseStream.EndRead(ar);

                    byte[] received = new byte[actualLength];

                    Buffer.BlockCopy(buffer, 0, received, 0, actualLength);

                    //raiseAppSerialDataEvent(received);
                    System.Console.WriteLine(received.ToHexString());
                }

                catch (IOException exc)
                {
                    System.Console.Write(exc);
                    //handleAppSerialError(exc);
                }

                read(serialPort);
            }, null);
        }

        static void PinChangedHandler(object sender, SerialPinChangedEventArgs e)
        {
            System.Console.WriteLine("Pin Changed:");
            System.Console.WriteLine(e.EventType.ToString());
        }

        static void ErrorReceivedHandler(object sender, SerialErrorReceivedEventArgs e)
        {
            System.Console.WriteLine("Error Received:");
            System.Console.Write(e.EventType.ToString());
       }

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            var sp = (SerialPort)sender;

            //System.Console.WriteLine(sp.ReadExisting());

            var buffer = new byte[sp.BytesToRead];
            sp.Read(buffer, 0, sp.BytesToRead);



            var data = buffer.ToDecString();

            //System.Console.WriteLine("Data Received:");
            System.Console.WriteLine(data);
        }
    }
}

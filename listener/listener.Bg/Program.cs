using System.ServiceProcess;

namespace listener.Bg
{
    static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new HostReaderService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}

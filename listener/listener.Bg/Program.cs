using System.Configuration;
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
                new HostReaderService(ConfigurationManager.AppSettings["endpoint"])
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}

using System;
using System.Threading.Tasks;
using listener.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Owin;

[assembly: OwinStartup(typeof(listener.Web.OwinStartup))]
namespace listener.Web
{
    public class OwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
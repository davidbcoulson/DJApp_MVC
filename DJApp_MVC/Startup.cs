using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DJApp_MVC.Startup))]
namespace DJApp_MVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

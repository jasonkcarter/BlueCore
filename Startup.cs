using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BlueCore.Startup))]
namespace BlueCore
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

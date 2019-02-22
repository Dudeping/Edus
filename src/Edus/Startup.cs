using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(XZJ_BS.Startup))]
namespace XZJ_BS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

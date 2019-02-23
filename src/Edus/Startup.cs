using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Edus.Startup))]
namespace Edus
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

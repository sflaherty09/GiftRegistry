using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GiftRegistry.Startup))]
namespace GiftRegistry
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RezaBaby.Startup))]
namespace RezaBaby
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

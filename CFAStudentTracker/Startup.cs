using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CFAStudentTracker.Startup))]
namespace CFAStudentTracker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

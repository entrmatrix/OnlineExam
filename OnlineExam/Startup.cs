using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OnlineExam.Startup))]
namespace OnlineExam
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

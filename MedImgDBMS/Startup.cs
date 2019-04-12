using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MedImgDBMS.Startup))]
namespace MedImgDBMS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

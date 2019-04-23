using System.Web;
using System.Web.Mvc;

namespace MedImgDBMS
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AuthorizeAttribute());  // Add authorize attribute for the whole site
        }
    }
}

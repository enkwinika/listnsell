using System.Web;
using System.Web.Mvc;
using rexell.Filters;

namespace rexell
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new GlobalExceptionFilter());
        }
    }
}

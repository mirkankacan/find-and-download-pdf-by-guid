using System.Web.Mvc;
using System.Web.Routing;

namespace IdeaPDFViewer
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                  name: "PdfViewer",
                  url: "{guidString}",
                  defaults: new { controller = "PDF", action = "Viewer", guidString = UrlParameter.Optional }
              );


        }
    }
}

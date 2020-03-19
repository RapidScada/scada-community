using System.Linq;
using System.Web.Http;

namespace GrafanaDataProvider
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var xmlFormatter = config.Formatters.XmlFormatter;
            var appXmlType = xmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            xmlFormatter.SupportedMediaTypes.Remove(appXmlType);
        }
    }
}

using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using ODataExamples.Repository.Model;

namespace ODataExamples.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            // OData Routing
            var apiBuilder = new ODataConventionModelBuilder{
                Namespace = "ODataSamples",
                ContainerName = "Container"
            };
            apiBuilder.EnableLowerCamelCase();

            // When adding new controllers, map in the ODataRoute paths here
            apiBuilder.EntitySet<Address>("addresses");
            apiBuilder.EntitySet<Customer>("customers");
            apiBuilder.EntitySet<Order>("orders");

            // Apply OData model/mappings
            config.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
            config.MapODataServiceRoute("odata", "odata", apiBuilder.GetEdmModel());
            config.EnsureInitialized();
        }
    }
}

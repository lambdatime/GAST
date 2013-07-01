using System.Web.Routing;
using Combres;

[assembly: WebActivator.PreApplicationStartMethod(typeof(GreatAmericanSolrTracker.Web.App_Start.Combres), "PreStart")]
namespace GreatAmericanSolrTracker.Web.App_Start {
    public static class Combres {
        public static void PreStart() {
            RouteTable.Routes.AddCombresRoute("Combres");
        }
    }
}
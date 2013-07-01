using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Routing;
using GreatAmericanSolrTracker.Web.Helpers;
using GreatAmericanSolrTracker.Web.Helpers.ModelBinders;
using GreatAmericanSolrTracker.Web.Models;

namespace GreatAmericanSolrTracker.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {

            RouteTable.Routes.MapHubs();

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Home",
                "Query/{solrCoreId}",
                new { controller = "Home", action = "Query"}
                ).RouteHandler = new MvcRouteHandler();

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            new ModelBinderRegistrar(new SmartBinder(), new [] { new SolrCoreDisplayBinder() }).RegisterBinders();

            

            InitDatabase();
        }

        void InitDatabase()
        {
            Database.SetInitializer<SolrCoreContext>(null);
        }
    }
}
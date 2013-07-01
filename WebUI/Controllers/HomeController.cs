using System;
using System.Net;
using System.Web.Mvc;
using GreatAmericanSolrTracker.Web.DisplayModels;

namespace GreatAmericanSolrTracker.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Query(SolrCoreDisplay solrCore, string queryString)
        {
            var queryResults = RunQuery(solrCore.BaseUrl, queryString);
            return View(new QueryDisplay(solrCore, queryString, queryResults));
        }

        private static string RunQuery(string coreUrl, string query)
        {
            var queryString = string.Format("q={0}&version=2.2&start=0&rows=10&indent=on&wt=json", query);
            var url = string.Format("{0}/select?{1}", coreUrl, Uri.EscapeUriString(queryString));

            using (var wc = new WebClient())
            {
                try
                {
                    wc.Encoding = System.Text.Encoding.UTF8;
                    wc.Headers["User-Agent"] = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; .NET4.0C; .NET4.0E)";
                    return wc.DownloadString(url);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}

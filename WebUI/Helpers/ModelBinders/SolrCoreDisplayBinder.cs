using System.Linq;
using System.Web.Mvc;
using GreatAmericanSolrTracker.Web.DisplayModels;
using GreatAmericanSolrTracker.Web.Models;

namespace GreatAmericanSolrTracker.Web.Helpers.ModelBinders
{
    public class SolrCoreDisplayBinder : TypedModelBinderBase<SolrCoreDisplay>
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var solrCoreId = bindingContext.GetValue<int>("solrCoreId");
            using (var context = new SolrCoreContext())
            {
                var solrCore = context.SolrCores.FirstOrDefault(c => c.SolrCoreId == solrCoreId);
                return new SolrCoreDisplay(solrCore);
            }
        }
    }
}
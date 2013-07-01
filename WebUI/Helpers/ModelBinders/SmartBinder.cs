using System.Linq;
using System.Web.Mvc;

namespace GreatAmericanSolrTracker.Web.Helpers.ModelBinders
{
    public class SmartBinder : DefaultModelBinder
    {
        private readonly IFilteredModelBinder[] _filteredModelBinders;

        public SmartBinder(params IFilteredModelBinder[] filteredModelBinders)
        {
            _filteredModelBinders = filteredModelBinders;
        }

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var filteredModelBinder = _filteredModelBinders.FirstOrDefault(binder => binder.IsMatch(bindingContext.ModelType));

            return filteredModelBinder == null
                       ? base.BindModel(controllerContext, bindingContext)
                       : filteredModelBinder.BindModel(controllerContext, bindingContext);
        }
    }
}
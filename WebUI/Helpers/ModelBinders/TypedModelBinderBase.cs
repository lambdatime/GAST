using System;
using System.Web.Mvc;

namespace GreatAmericanSolrTracker.Web.Helpers.ModelBinders
{
    public abstract class TypedModelBinderBase<TModel> : ITypedModelBinder
    {
        public abstract object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext);

        public Type GetBindingType()
        {
            return typeof(TModel);
        }
    }
}
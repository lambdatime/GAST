using System;
using System.Web.Mvc;

namespace GreatAmericanSolrTracker.Web.Helpers.ModelBinders
{
    public interface ITypedModelBinder : IModelBinder
    {
        Type GetBindingType();
    }
}
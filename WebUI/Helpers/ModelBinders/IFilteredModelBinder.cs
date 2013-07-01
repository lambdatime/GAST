using System;
using System.Web.Mvc;

namespace GreatAmericanSolrTracker.Web.Helpers.ModelBinders
{
    public interface IFilteredModelBinder : IModelBinder
    {
        bool IsMatch(Type modelType);
    }
}
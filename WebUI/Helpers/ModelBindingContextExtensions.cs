using System;
using System.Web.Mvc;
using GreatAmericanSolrTracker.Web.Helpers.ModelBinders;

namespace GreatAmericanSolrTracker.Web.Helpers
{
    public class ModelBinderRegistrar
    {
        private readonly IModelBinder _defaultBinder;
        private readonly ITypedModelBinder[] _typedBinders;

        public ModelBinderRegistrar(IModelBinder defaultBinder, params ITypedModelBinder[] typedBinders)
        {
            _defaultBinder = defaultBinder;
            _typedBinders = typedBinders;
        }

        public void RegisterBinders()
        {
            foreach (var typedModelBinder in _typedBinders)
            {
                System.Web.Mvc.ModelBinders.Binders.Add(typedModelBinder.GetBindingType(), typedModelBinder);
            }

            System.Web.Mvc.ModelBinders.Binders.DefaultBinder = _defaultBinder;
        }
    }
    public static class ModelBindingContextExtensions
    {
        public static TValue GetValue<TValue>(this ModelBindingContext context)
        {
            return GetValue<TValue>(context, context.ModelName);
        }

        public static TValue GetValue<TValue>(this ModelBindingContext context, string key)
        {
            return (TValue)context.ValueProvider.GetValue(key).ConvertTo(typeof(TValue));
        }

        public static TValue TryGetValue<TValue>(this ModelBindingContext context)
        {
            return TryGetValue<TValue>(context, context.ModelName);
        }

        public static TValue TryGetValue<TValue>(this ModelBindingContext context, string key)
        {
            var defaultTValue = default(TValue);
            var valueProviderResult = context.ValueProvider.GetValue(key);

            if (valueProviderResult == null) return defaultTValue;

            //must be checked due to possible conversion exception, 
            // format exceptions can occur if a string cannot convert to a specific type
            try
            {
                return (TValue)valueProviderResult.ConvertTo(typeof(TValue));
            }
            catch (InvalidOperationException)
            {
                return defaultTValue;
            }
        }
    }
}
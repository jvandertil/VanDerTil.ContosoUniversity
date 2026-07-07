using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Razor;

namespace VanDerTil.ContosoUniversity.Web.Infrastructure;

public sealed class FeatureFolderViewLocationExpander : IViewLocationExpander
{
    private const string FeatureFolderKey = "FeaturePath";

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        if (!context.Values.TryGetValue(FeatureFolderKey, out var featurePath))
        {
            return viewLocations;
        }

        var expandedLocations = new[]
        {
            $"~/Features/{featurePath}/Views/{{0}}.cshtml",
            $"~/Features/{featurePath}/{{0}}.cshtml",
            $"~/Features/Shared/{{0}}.cshtml"
        };

        return expandedLocations.AsEnumerable().Concat(viewLocations);
    }

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        var controllerAction = (ControllerActionDescriptor)context.ActionContext.ActionDescriptor;
        var controllerNamespace = controllerAction.ControllerTypeInfo.Namespace;

        if (string.IsNullOrEmpty(controllerNamespace))
        {
            return;
        }

        var featureMarker = "Features.";
        var featureIndex = controllerNamespace.IndexOf(featureMarker, StringComparison.Ordinal);

        if (featureIndex < 0)
        {
            return;
        }

        var pathAfterFeatures = controllerNamespace[(featureIndex + featureMarker.Length)..];
        var featurePath = pathAfterFeatures.Replace(".", "/");

        context.Values[FeatureFolderKey] = featurePath;
    }
}

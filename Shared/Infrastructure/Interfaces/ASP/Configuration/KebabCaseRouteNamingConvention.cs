using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace SafeFlow.API.Shared.Infrastructure.Interfaces.ASP.Configuration;

public class KebabCaseRouteNamingConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        foreach (var selector in controller.Selectors)
            selector.AttributeRouteModel = ReplaceControllerTemplate(selector, controller.ControllerName);

        foreach (var action in controller.Actions)
        foreach (var selector in action.Selectors)
            selector.AttributeRouteModel = ReplaceControllerTemplate(selector, controller.ControllerName);
    }

    private static AttributeRouteModel? ReplaceControllerTemplate(
        SelectorModel selector, string controllerName)
    {
        if (selector.AttributeRouteModel?.Template == null) return selector.AttributeRouteModel;
        var template = selector.AttributeRouteModel.Template
            .Replace("[controller]", ToKebabCase(controllerName.Replace("Controller", "")));
        return new AttributeRouteModel { Template = template };
    }

    private static string ToKebabCase(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return string.Concat(value.Select((ch, i) =>
            i > 0 && char.IsUpper(ch) ? "-" + char.ToLower(ch) : char.ToLower(ch).ToString()));
    }
}

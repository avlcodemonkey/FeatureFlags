using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Services;

public interface IViewService {
    Task<string> RenderViewToStringAsync(string viewName, object model, ControllerContext context, bool isPartial = false);
}

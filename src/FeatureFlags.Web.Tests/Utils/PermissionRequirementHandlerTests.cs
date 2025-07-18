using System.Security.Claims;
using FeatureFlags.Controllers;
using FeatureFlags.Extensions;
using FeatureFlags.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace FeatureFlags.Web.Tests.Utils;

/// <summary>
/// Testing that the custom authorization handler works correctly.
/// Tests invoke `HandleAsync` which just calls `HandleRequirementAsync`.
/// </summary>
public class PermissionRequirementHandlerTests {
    private readonly PermissionRequirementHandler _PermissionRequirementHandler;
    private readonly PermissionRequirement _PermissionRequirement;

    public PermissionRequirementHandlerTests() {
        _PermissionRequirement = new PermissionRequirement();
        _PermissionRequirementHandler = new PermissionRequirementHandler();
    }

    [Fact]
    public void HandleAsync_WithNoResource_ReturnsEarly() {
        // arrange
        var claimsIdentity = new ClaimsIdentity([new Claim(ClaimTypes.Role, "dashboard.index")]);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        // setting resource=null should trigger the handler to fail
        var authorizationHandlerContext = new AuthorizationHandlerContext([_PermissionRequirement], claimsPrincipal, null);

        // act
        var result = _PermissionRequirementHandler.HandleAsync(authorizationHandlerContext);

        // assert
        Assert.Equal(Task.CompletedTask, result);
        Assert.False(authorizationHandlerContext.HasSucceeded);
        Assert.Contains(_PermissionRequirement, authorizationHandlerContext.PendingRequirements);
    }

    [Fact]
    public void HandleAsync_WithoutPermissionToAction_ReturnsFailure() {
        // arrange
        var claimsIdentity = new ClaimsIdentity([new Claim(ClaimTypes.Role, "controller.action")]);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var descriptor = new ControllerActionDescriptor {
            ControllerName = nameof(DashboardController).StripController(), ActionName = nameof(DashboardController.Index),
            MethodInfo = typeof(DashboardController).GetMethod(nameof(DashboardController.Index))!
        };
        var endpoint = new Endpoint(x => Task.CompletedTask, new EndpointMetadataCollection(descriptor), null);
        var authorizationHandlerContext = new AuthorizationHandlerContext([_PermissionRequirement], claimsPrincipal, endpoint);

        // act
        var result = _PermissionRequirementHandler.HandleAsync(authorizationHandlerContext);

        // assert
        Assert.Equal(Task.CompletedTask, result);
        Assert.False(authorizationHandlerContext.HasSucceeded);
        // requirement will still be pending since the handler doesn't explicitly fail the requirement
        Assert.Contains(_PermissionRequirement, authorizationHandlerContext.PendingRequirements);
    }

    [Fact]
    public void HandleAsync_WithPermissionToAction_ReturnsSuccess() {
        // arrange
        var claimsIdentity = new ClaimsIdentity([new Claim(ClaimTypes.Role, "dashboard.index")]);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var descriptor = new ControllerActionDescriptor {
            ControllerName = nameof(DashboardController).StripController(), ActionName = nameof(DashboardController.Index),
            MethodInfo = typeof(DashboardController).GetMethod(nameof(DashboardController.Index))!
        };
        var endpoint = new Endpoint(x => Task.CompletedTask, new EndpointMetadataCollection(descriptor), null);
        var authorizationHandlerContext = new AuthorizationHandlerContext([_PermissionRequirement], claimsPrincipal, endpoint);

        // act
        var result = _PermissionRequirementHandler.HandleAsync(authorizationHandlerContext);

        // assert
        Assert.Equal(Task.CompletedTask, result);
        Assert.True(authorizationHandlerContext.HasSucceeded);
        Assert.DoesNotContain(_PermissionRequirement, authorizationHandlerContext.PendingRequirements);
    }

    [Fact]
    public void HandleAsync_WithoutPermissionToChildOrParent_ReturnsFailure() {
        // arrange
        var claimsIdentity = new ClaimsIdentity([new Claim(ClaimTypes.Role, "controller.action")]);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var descriptor = new ControllerActionDescriptor {
            ControllerName = nameof(AccountController).StripController(), ActionName = nameof(AccountController.ToggleContextHelp),
            MethodInfo = typeof(AccountController).GetMethod(nameof(AccountController.ToggleContextHelp))!
        };
        var endpoint = new Endpoint(x => Task.CompletedTask, new EndpointMetadataCollection(descriptor), null);
        var authorizationHandlerContext = new AuthorizationHandlerContext([_PermissionRequirement], claimsPrincipal, endpoint);

        // act
        var result = _PermissionRequirementHandler.HandleAsync(authorizationHandlerContext);

        // assert
        Assert.Equal(Task.CompletedTask, result);
        Assert.False(authorizationHandlerContext.HasSucceeded);
        // requirement will still be pending since the handler doesn't explicitly fail the requirement
        Assert.Contains(_PermissionRequirement, authorizationHandlerContext.PendingRequirements);
    }

    [Fact]
    public void HandleAsync_WithPermissionToParent_ReturnsSuccess() {
        // arrange
        var claimsIdentity = new ClaimsIdentity([new Claim(ClaimTypes.Role, "account.index")]);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var descriptor = new ControllerActionDescriptor {
            ControllerName = nameof(AccountController).StripController(), ActionName = nameof(AccountController.ToggleContextHelp),
            MethodInfo = typeof(AccountController).GetMethod(nameof(AccountController.ToggleContextHelp))!
        };
        var endpoint = new Endpoint(x => Task.CompletedTask, new EndpointMetadataCollection(descriptor), null);
        var authorizationHandlerContext = new AuthorizationHandlerContext([_PermissionRequirement], claimsPrincipal, endpoint);

        // act
        var result = _PermissionRequirementHandler.HandleAsync(authorizationHandlerContext);

        // assert
        Assert.Equal(Task.CompletedTask, result);
        Assert.True(authorizationHandlerContext.HasSucceeded);
        Assert.DoesNotContain(_PermissionRequirement, authorizationHandlerContext.PendingRequirements);
    }

    [Fact]
    public void HandleAsync_WithPermissionToChild_ReturnsFailure() {
        // this case should never occur since an action with a parent action should not have it's own permission in the db
        // but testing it to make sure the authorization handler handles that just in case 
        // arrange
        var claimsIdentity = new ClaimsIdentity([new Claim(ClaimTypes.Role, "account.togglecontexthelp")]);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var descriptor = new ControllerActionDescriptor {
            ControllerName = nameof(AccountController).StripController(), ActionName = nameof(AccountController.ToggleContextHelp),
            MethodInfo = typeof(AccountController).GetMethod(nameof(AccountController.ToggleContextHelp))!
        };
        var endpoint = new Endpoint(x => Task.CompletedTask, new EndpointMetadataCollection(descriptor), null);
        var authorizationHandlerContext = new AuthorizationHandlerContext([_PermissionRequirement], claimsPrincipal, endpoint);

        // act
        var result = _PermissionRequirementHandler.HandleAsync(authorizationHandlerContext);

        // assert
        Assert.Equal(Task.CompletedTask, result);
        Assert.False(authorizationHandlerContext.HasSucceeded);
        // requirement will still be pending since the handler doesn't explicitly fail the requirement
        Assert.Contains(_PermissionRequirement, authorizationHandlerContext.PendingRequirements);
    }

}

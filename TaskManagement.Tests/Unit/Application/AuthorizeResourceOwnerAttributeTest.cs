using System.Runtime.CompilerServices;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using TaskManagement.Application.Attributes;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Infrastructure.AuthHandlers;
using Xunit;

namespace TaskManagement.Tests.Unit;

public class AuthorizeResourceOwnerAttributeTest
{
    private readonly Mock<IAuthorizationService> _authServiceMock = new();
    private readonly Mock<IResourceOwnerService> _resourceOwnerServiceMock = new();

    private ActionExecutingContext BuildContext(
        Guid? routeId,
        ClaimsPrincipal user,
        string paramName = "id")
    {
        var httpContext = new DefaultHttpContext();
        httpContext.User = user;

        var services = new ServiceCollection();
        services.AddSingleton(_authServiceMock.Object);
        services.AddSingleton(_resourceOwnerServiceMock.Object);
        httpContext.RequestServices = services.BuildServiceProvider();

        var actionDescriptor = new ControllerActionDescriptor();
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        var args = new Dictionary<string, object?>();
        if (routeId.HasValue)
        {
            args[paramName] = routeId.Value;
        }

        return new ActionExecutingContext(actionContext, [], args, new object());
    }

    private static ActionExecutionDelegate NoNextOperation()
        => () => Task.FromResult<ActionExecutedContext>(null!);

    [Fact]
    public async Task WhenRouteIdMissing_ShouldReturnBadRequest()
    {
        var filter = new AuthorizeResourceOwnerAttribute<IResourceOwnerService>();
        var context = BuildContext(routeId: null, user: new ClaimsPrincipal());

        await filter.OnActionExecutionAsync(context, NoNextOperation());

        context.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task WhenUserNotOwnerOrAdmin_ShouldReturn403Forbiden()
    {
        var resourceId = Guid.NewGuid();
        var realOwnerId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())]));

        _resourceOwnerServiceMock
            .Setup(s => s.GetOwnerIdAsync(resourceId))
            .ReturnsAsync(realOwnerId);

        _authServiceMock
            .Setup(s => s.AuthorizeAsync(
                user,
                realOwnerId,
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Failed());

        var filter = new AuthorizeResourceOwnerAttribute<IResourceOwnerService>(resourceName: "task");
        var context = BuildContext(resourceId, user);

        await filter.OnActionExecutionAsync(context, NoNextOperation());

        context.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task WhenUserOwnsResource_ShouldCallNextOperation()
    {
        var resourceId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, ownerId.ToString())]));

        _resourceOwnerServiceMock
            .Setup(s => s.GetOwnerIdAsync(resourceId))
            .ReturnsAsync(ownerId);

        _authServiceMock
            .Setup(s => s.AuthorizeAsync(
                user,
                ownerId,
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var filter = new AuthorizeResourceOwnerAttribute<IResourceOwnerService>(resourceName: "task");
        var context = BuildContext(resourceId, user);

        var nextOperationCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextOperationCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        };

        await filter.OnActionExecutionAsync(context, next);

        nextOperationCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }
}
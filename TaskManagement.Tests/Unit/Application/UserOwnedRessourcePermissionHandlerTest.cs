using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using TaskManagement.Application.Common.Constants;
using TaskManagement.Infrastructure.AuthHandlers;
using Xunit;

namespace TaskManagement.Tests;

public class UserOwnedResourcePermissionHandlerTest
{
    private readonly UserOwnedResourcePermissionHandler _handler = new();
    private readonly UserOwnedResourcePermission _requirement = new();

    private static ClaimsPrincipal BuildUser(Guid userId, bool isAdmin = false)
    {
        return new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, isAdmin ? Role.AdminRole : Role.UserRole),
                ],
                "Bearer"
            )
        );
    }

    [Fact]
    public async Task WhenUserAdmin_ShouldSucceed()
    {
        var realOwnerId = Guid.NewGuid();

        var user = BuildUser(Guid.NewGuid(), true);

        AuthorizationHandlerContext authContext = new([_requirement], user, realOwnerId);

        await _handler.HandleAsync(authContext);
        authContext.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task WhenUserOwnerAndNotAdmin_ShouldShouldSucceed()
    {
        var realOwnerId = Guid.NewGuid();

        var user = BuildUser(realOwnerId, false);

        AuthorizationHandlerContext authContext = new([_requirement], user, realOwnerId);

        await _handler.HandleAsync(authContext);
        authContext.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task WhenUserNotOwnerAndNotAdmin_ShouldFail()
    {
        var realOwnerId = Guid.NewGuid();

        var user = BuildUser(Guid.NewGuid(), false);

        AuthorizationHandlerContext authContext = new([_requirement], user, realOwnerId);

        await _handler.HandleAsync(authContext);
        authContext.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task WhenUserHasNoIdentityClaims_ShouldFail()
    {
        var authContext = new AuthorizationHandlerContext(
            [_requirement],
            new ClaimsPrincipal(),
            Guid.NewGuid()
        );

        await _handler.HandleAsync(authContext);

        authContext.HasSucceeded.Should().BeFalse();
    }
}

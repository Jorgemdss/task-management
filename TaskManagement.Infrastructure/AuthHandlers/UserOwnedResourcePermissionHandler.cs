using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TaskManagement.Application.Common.Constants;
using TaskManagement.Domain.Models;

namespace TaskManagement.Infrastructure.AuthHandlers;

public class UserOwnedResourcePermissionHandler
    : AuthorizationHandler<UserOwnedResourcePermission, Guid>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserOwnedResourcePermission requirement,
        Guid ownerId
    )
    {
        if (IsAdmin(context.User) || IsOwner(context.User, ownerId))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private static bool IsOwner(ClaimsPrincipal user, Guid ownerId)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return false;
        }

        return userId == ownerId;
    }

    private static bool IsAdmin(ClaimsPrincipal user)
    {
        return user.IsInRole(Role.AdminRole);
    }
}

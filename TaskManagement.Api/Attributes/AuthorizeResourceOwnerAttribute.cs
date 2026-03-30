using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Infrastructure.AuthHandlers;

namespace TaskManagement.Application.Attributes;


public class AuthorizeResourceOwnerAttribute<TService>(string idParameterName = "id", string resourceName = "resource") : Attribute, IAsyncActionFilter where TService : IResourceOwnerService
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var service = context.HttpContext.RequestServices.GetRequiredService<TService>();
        var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

        if (!context.ActionArguments.TryGetValue(idParameterName, out var entityId) || entityId is not Guid resourceId)
        {
            context.Result = new BadRequestObjectResult("Resource ID is required");
            return;
        }

        var ownerId = await service.GetOwnerIdAsync(resourceId);
        var auth = await authService.AuthorizeAsync(context.HttpContext.User, ownerId, new UserOwnedResourcePermission());

        if (!auth.Succeeded)
        {
            context.Result = new ObjectResult(new { message = $"You don't have access to the {resourceName} {resourceId}." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }


        await next();
    }
}
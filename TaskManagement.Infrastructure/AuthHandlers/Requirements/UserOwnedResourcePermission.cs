
using Microsoft.AspNetCore.Authorization;

namespace TaskManagement.Infrastructure.AuthHandlers;

public class UserOwnedResourcePermission : IAuthorizationRequirement
{

    public UserOwnedResourcePermission()
    {
    }

}
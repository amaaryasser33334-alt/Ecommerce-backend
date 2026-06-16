using Ecommerce_Backend.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public class UserOwnerOrAdminHandler
    : AuthorizationHandler<UserOwnerOrAdminRequirement, int> // ✅ int بدل string
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserOwnerOrAdminRequirement requirement,
        int userId) // ✅ int
    {
        var currentUserId = context.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        bool isAdmin = context.User.IsInRole("Admin");

        if (isAdmin || currentUserId == userId.ToString())
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
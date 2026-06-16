using Microsoft.AspNetCore.Authorization;

namespace Ecommerce_Backend.Authorization;

public class UserOwnerOrAdminRequirement
    : IAuthorizationRequirement
{
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using omnicart_api.Models;
using omnicart_api.Requests;
using omnicart_api.Services;

namespace omnicart_api.Controllers.Csr;

[Route("api/csr/users")]
[ApiController]
[ServiceFilter(typeof(ValidateModelAttribute))]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    /// <summary>
    /// Initializes the UsersController with MongoDbService dependency.
    /// </summary>
    /// <param name="userService">The MongoDB service</param>
    public UserController(UserService userService)
    {
        _userService = userService;
    }


    // Activate/Deactivate a product
    [HttpPatch("{id:length(24)}/activate")]
    [Authorize(Roles = "csr,admin")]
    public async Task<ActionResult<AppResponse<User>>> SetUserStatus(string id, [FromBody] UpdateUserStatusDto status)
    {
        var user = await _userService.GetUserByIdAsync(id);

        if (user == null)
            return NotFound(new AppResponse<User> { Success = false, Message = "User not found" });

        await _userService.SetUserStatusAsync(id, status.IsActive);

        user.IsActive = status.IsActive;

        return Ok(new AppResponse<User> { Success = true, Data = user, Message = $"User status updated to {status.IsActive}" });
    }
}
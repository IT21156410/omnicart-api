// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Handle HTTP API requests related to customer User management. 
// ***********************************************************************

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using omnicart_api.Models;
using omnicart_api.Requests;
using omnicart_api.Services;

namespace omnicart_api.Controllers;

[Route("api/customer/users")]
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

    /// <summary>
    /// Handles PUT requests to update a user profile
    /// </summary>
    /// <param name="updatedUser"></param>
    /// <returns>AppResponse result if successful</returns>
    [Authorize(Roles = "customer")]
    [HttpPost]
    public async Task<ActionResult<AppResponse<User>>> Update(UpdateProfileUserDto updatedUser)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized(new AppResponse<string>
            {
                Success = false,
                Message = "User is not authenticated",
                ErrorCode = 401
            });
        }

        var loggedUser = await _userService.GetUserByIdAsync(userId);

        if (loggedUser == null)
        {
            return NotFound(new AppResponse<User>
            {
                Success = false,
                Message = "User not found",
                ErrorCode = 404
            });
        }

        loggedUser.Name = updatedUser.Name;
        loggedUser.Email = updatedUser.Email;

        await _userService.UpdateUserAsync(userId, loggedUser);

        return Ok(new AppResponse<User>
        {
            Success = true,
            Data = loggedUser,
            Message = "User profile updated successfully"
        });
    }

    // Activate/Deactivate a product
    [HttpPatch("deactivate")]
    [Authorize(Roles = "customer")]
    public async Task<ActionResult<AppResponse<User>>> DeactivateTheProfile()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized(new AppResponse<string>
            {
                Success = false,
                Message = "User is not authenticated",
                ErrorCode = 401
            });
        }

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new AppResponse<string>
            {
                Success = false,
                Message = "User not found",
                ErrorCode = 404
            });
        } 
        
        await _userService.SetUserStatusAsync(userId, false);

        user.IsActive = false;

        return Ok(new AppResponse<User> { Success = true, Data = user, Message = $"User profile deactivated successfully, Contact CSR for activation" });
    }
}
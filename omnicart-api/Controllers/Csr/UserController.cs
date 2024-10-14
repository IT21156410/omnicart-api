// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Handle HTTP API requests related to CSR User managment. 
// ***********************************************************************

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

    /// <summary>
    /// Handles GET requests to retrieve all users
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Authorize(Roles = "csr,admin")]
    public async Task<ActionResult<AppResponse<List<User>>>> Get()
    {
        try
        {
            var users = await _userService.GetUsersAsync();
            var response = new AppResponse<List<User>>
            {
                Success = true,
                Data = users,
                Message = "Users retrieved successfully"
            };
            return response;
        }
        catch (Exception ex)
        {
            var response = new AppResponse<List<User>>
            {
                Success = false,
                Data = [],
                Message = "An error occurred while retrieving users",
                Error = ex.Message,
                ErrorCode = 500
            };

            return StatusCode(500, response);
        }
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
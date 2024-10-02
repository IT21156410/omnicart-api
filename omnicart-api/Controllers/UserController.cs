// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Handle HTTP API requests related to user management.
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// ***********************************************************************

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using omnicart_api.Models;
using omnicart_api.Requests;
using omnicart_api.Services;

// https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio

namespace omnicart_api.Controllers
{
    [Route("api/users")]
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
            catch (System.Exception ex)
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

        /// <summary>
        ///  Handles GET requests to retrieve a specific user by ID.
        /// </summary>
        /// <param name="id">The ObjectId of the user</param>
        /// <returns>The user object if found</returns>
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<AppResponse<User>>> Get(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(new AppResponse<User>
                {
                    Success = false,
                    Message = "User not found",
                    ErrorCode = 404
                });
                
            }

            return Ok(new AppResponse<User>
            {
                Success = true,
                Data = user,
                Message = "User retrieved successfully"
            });
        }

        /// <summary>
        /// Handles POST requests to create a new user in the MongoDB collection.
        /// </summary>
        /// <param name="newUser">The new user object</param>
        /// <returns>CreatedAtAction result with the new user</returns>
        [HttpPost]
        public async Task<ActionResult<AppResponse<User>>> Post(User newUser)
        {
            await _userService.CreateUserAsync(newUser);

            return CreatedAtAction(nameof(Get), new { id = newUser.Id }, new AppResponse<User>
            {
                Success = true,
                Data = newUser,
                Message = "User created successfully"
            });
        }

        /// <summary>
        /// Handles PUT requests to update an existing user
        /// </summary>
        /// <param name="id">The ObjectId of the user</param>
        /// <param name="updatedUser"></param>
        /// <returns>ActionResult<AppResponse<User>> result if successful</returns>
        [HttpPut("{id:length(24)}")]
        public async Task<ActionResult<AppResponse<User>>> Update(string id, UpdateUserDto updatedUser)
        {
            var existingUser = await _userService.GetUserByIdAsync(id);

            if (existingUser == null)
            {
                return NotFound(new AppResponse<User>
                {
                    Success = false,
                    Message = "User not found",
                    ErrorCode = 404
                });
            }
             
            existingUser.Name = updatedUser.Name;
            existingUser.Email = updatedUser.Email;
            existingUser.Role = updatedUser.Role ?? existingUser.Role;
            existingUser.Password = existingUser.Password;

            await _userService.UpdateUserAsync(id, existingUser);

            return Ok(new AppResponse<User>
            {
                Success = true,
                Data = existingUser,
                Message = "User updated successfully"
            });
        }

        /// <summary>
        /// Handles DELETE requests to remove a user
        /// </summary>
        /// <param name="id">The ObjectId of the user</param>
        /// <returns>NoContent result if successful</returns>
        [HttpDelete("{id:length(24)}")]
        public async Task<ActionResult<AppResponse<User>>> Delete(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(new AppResponse<string>
                {
                    Success = false,
                    Message = "User not found",
                    ErrorCode = 404
                });
            }

            await _userService.DeleteUserAsync(id);

            return Ok(new AppResponse<User>
            {
                Success = true,
                Data = user,
                Message = "User deleted successfully"
            });
        }
    }
}

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
using omnicart_api.Services;

// https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio

namespace omnicart_api.Controllers
{
    [Route("api/users")]
    [ApiController]
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
        public async Task<List<User>> Get() =>
            await _userService.GetUsersAsync();

        /// <summary>
        ///  Handles GET requests to retrieve a specific user by ID.
        /// </summary>
        /// <param name="id">The ObjectId of the user</param>
        /// <returns>The user object if found</returns>
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<User>> Get(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        /// <summary>
        /// Handles POST requests to create a new user in the MongoDB collection.
        /// </summary>
        /// <param name="newUser">The new user object</param>
        /// <returns>CreatedAtAction result with the new user</returns>
        [HttpPost]
        public async Task<IActionResult> Post(User newUser)
        {
            await _userService.CreateUserAsync(newUser);

            return CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
        }

        /// <summary>
        /// Handles PUT requests to update an existing user
        /// </summary>
        /// <param name="id">The ObjectId of the user</param>
        /// <param name="updatedUser"></param>
        /// <returns>NoContent result if successful</returns>
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, User updatedUser)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            updatedUser.Id = user.Id;

            await _userService.UpdateUserAsync(id, updatedUser);

            return NoContent();
        }

        /// <summary>
        /// Handles DELETE requests to remove a user
        /// </summary>
        /// <param name="id">The ObjectId of the user</param>
        /// <returns>NoContent result if successful</returns>
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            await _userService.DeleteUserAsync(id);

            return NoContent();
        }
    }
}

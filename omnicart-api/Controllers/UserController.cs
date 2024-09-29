using omnicart_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace omnicart_api.Controllers
{
    public class UserController : ApiController
    {
        private static List<User> users = new List<User>
        {
            new User { id = 1, name = "John Doe", email = "john@example.com", password = "password123",role = "admin" }
        };

        [HttpGet]
        [Route("api/users")]
        public IHttpActionResult GetUsers()
        {
            return Ok(users);
        }

        // GET: api/users/{id}
        [HttpGet]
        [Route("api/users/{id}", Name = "GetUser")]
        public IHttpActionResult GetUser(int id)
        {
            var user = users.FirstOrDefault(u => u.id == id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        // POST: api/users
        [HttpPost]
        [Route("api/users")]
        public IHttpActionResult CreateUser([FromBody] User user)
        {
            if (user == null || string.IsNullOrEmpty(user.name) || string.IsNullOrEmpty(user.email))
            {
                return BadRequest("Invalid user data.");
            }

            user.id = users.Count + 1; // Simple ID generation
            users.Add(user);
            return CreatedAtRoute("GetUser", new { id = user.id }, user);
        }

        // PUT: api/users/{id}
        [HttpPut]
        [Route("api/users/{id}")]
        public IHttpActionResult UpdateUser(int id, [FromBody] User updatedUser)
        {
            if (updatedUser == null)
            {
                return BadRequest("Invalid user data.");
            }

            var user = users.FirstOrDefault(u => u.id == id);
            if (user == null)
            {
                return NotFound();
            }

            user.name = updatedUser.name;
            user.email = updatedUser.email;
            user.password = updatedUser.password; // Ensure proper hashing in production
            return Ok(user);
        }

        // DELETE: api/user/{id}
        [HttpDelete]
        [Route("api/users/{id}")]
        public IHttpActionResult DeleteUser(int id)
        {
            var user = users.FirstOrDefault(u => u.id == id);
            if (user == null)
            {
                return NotFound();
            }

            users.Remove(user);
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}

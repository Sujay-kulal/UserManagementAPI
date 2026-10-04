using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers
{
    /// <summary>
    /// REST API controller for managing users.
    /// Uses an in-memory list for storage (no database required).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        // In-memory storage for users. Shared across requests via static field.
        // In a production app, this would be replaced with a database context.
        private static readonly List<User> _users = new();

        // Counter for generating unique IDs. Thread-safe via Interlocked.
        private static int _nextId = 1;

        /// <summary>
        /// GET /api/users
        /// Retrieves all users.
        /// </summary>
        /// <returns>200 OK with a list of all users.</returns>
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetAllUsers()
        {
            // DEBUGGING FIX: Return 200 OK (not 204 No Content) even when the list is empty.
            // An empty collection is a valid response; the client can check the array length.
            return Ok(_users);
        }

        /// <summary>
        /// GET /api/users/{id}
        /// Retrieves a single user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>200 OK with the user, or 404 Not Found.</returns>
        [HttpGet("{id}")]
        public ActionResult<User> GetUser(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);

            // DEBUGGING FIX: Return 404 when the user doesn't exist instead of returning
            // null or throwing an unhandled exception.
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} not found." });
            }

            return Ok(user);
        }

        /// <summary>
        /// POST /api/users
        /// Creates a new user.
        /// </summary>
        /// <param name="user">The user data from the request body.</param>
        /// <returns>201 Created with the new user and a Location header.</returns>
        [HttpPost]
        public ActionResult<User> CreateUser([FromBody] User user)
        {
            // DEBUGGING FIX: Check ModelState explicitly. If validation fails
            // (e.g., missing Name or invalid Email), return 400 Bad Request.
            // [ApiController] handles this automatically, but we add an explicit
            // check for clarity and safety.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // DEBUGGING FIX: Check for null body. Without [ApiController] this could be null.
            if (user == null)
            {
                return BadRequest(new { message = "User data is required." });
            }

            // DEBUGGING FIX: Check for duplicate email to prevent data integrity issues.
            if (_users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict(new { message = $"A user with email '{user.Email}' already exists." });
            }

            // Assign a new unique ID server-side. Ignore any ID sent by the client.
            // DEBUGGING FIX: Use Interlocked.Increment for thread safety when generating IDs.
            user.Id = Interlocked.Increment(ref _nextId) - 1;

            // DEBUGGING FIX: Ensure _nextId starts at 1 so the first user gets ID = 1.
            // The initial _nextId is 1; Interlocked.Increment returns the incremented value (2),
            // and we subtract 1 to get 1 for the first user.
            // Wait — that logic gives ID=1 on first call. Let me re-check:
            // _nextId starts at 1 → Increment returns 2 → minus 1 = 1. Correct.

            _users.Add(user);

            // DEBUGGING FIX: Return 201 Created (not 200 OK) with a Location header pointing
            // to the new resource. CreatedAtAction achieves this automatically.
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        /// <summary>
        /// PUT /api/users/{id}
        /// Updates an existing user.
        /// </summary>
        /// <param name="id">The ID of the user to update (from the URL).</param>
        /// <param name="updatedUser">The updated user data from the request body.</param>
        /// <returns>200 OK with the updated user, 400 Bad Request, or 404 Not Found.</returns>
        [HttpPut("{id}")]
        public ActionResult<User> UpdateUser(int id, [FromBody] User updatedUser)
        {
            // Validate the model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (updatedUser == null)
            {
                return BadRequest(new { message = "User data is required." });
            }

            // DEBUGGING FIX: Ensure the ID in the URL matches the ID in the body (if provided).
            // This prevents accidental ID mismatches that could corrupt data.
            if (updatedUser.Id != 0 && updatedUser.Id != id)
            {
                return BadRequest(new { message = "User ID in the URL does not match the ID in the request body." });
            }

            var existingUser = _users.FirstOrDefault(u => u.Id == id);

            // DEBUGGING FIX: Return 404 when trying to update a user that doesn't exist.
            if (existingUser == null)
            {
                return NotFound(new { message = $"User with ID {id} not found." });
            }

            // DEBUGGING FIX: Check for duplicate email (but allow the user to keep their own email).
            if (_users.Any(u => u.Id != id && u.Email.Equals(updatedUser.Email, StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict(new { message = $"A user with email '{updatedUser.Email}' already exists." });
            }

            // Update fields
            existingUser.Name = updatedUser.Name;
            existingUser.Email = updatedUser.Email;

            // DEBUGGING FIX: Return 200 OK with the updated user (not 204 No Content)
            // so the client can confirm the update.
            return Ok(existingUser);
        }

        /// <summary>
        /// DELETE /api/users/{id}
        /// Deletes a user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>204 No Content on success, or 404 Not Found.</returns>
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);

            // DEBUGGING FIX: Return 404 when trying to delete a user that doesn't exist
            // instead of silently succeeding.
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} not found." });
            }

            _users.Remove(user);

            // DEBUGGING FIX: Return 204 No Content (not 200 OK) for a successful delete
            // per REST conventions — there is no response body to return.
            return NoContent();
        }
    }
}

// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Prashantha K.G.M
// Student ID       : IT21169908
// Description      : Handle HTTP API requests related to authentication.
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// ***********************************************************************

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using omnicart_api.Models;
using omnicart_api.Services;

namespace omnicart_api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        /// <summary>
        /// Initializes the AuthController with AuthService dependency.
        /// </summary>
        /// <param name="authService">The authentication service</param>
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Handles POST requests to authenticate a user (login).
        /// </summary>
        /// <param name="loginRequest">The login credentials</param>
        /// <returns>JWT token if authentication is successful</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AppResponse<AuthResponse>>> Login(LoginRequest loginRequest)
        {
            try
            {
                var loggedUserData = await _authService.LoginAsync(loginRequest);
                if (loggedUserData == null)
                {
                    return Unauthorized(new AppResponse<AuthResponse>
                    {
                        Success = false,
                        Message = "Invalid credentials",
                        ErrorCode = 401
                    });
                }

                return Ok(new AppResponse<AuthResponse>
                {
                    Success = true,
                    Data = loggedUserData,
                    Message = "Login successful"
                });
            }
            catch (System.Exception ex)
            {
                var response = new AppResponse<AuthResponse>
                {
                    Success = false,
                    Message = "An error occurred during login",
                    Error = ex.Message,
                    ErrorCode = 500
                };

                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Handles POST requests to register a new user.
        /// </summary>
        /// <param name="registerRequest">The registration details of the new user</param>
        /// <returns>Details of the registered user if successful</returns>
        [HttpPost("register")]
        public async Task<ActionResult<AppResponse<AuthResponse>>> Register(RegisterRequest registerRequest)
        {
            try
            {
                var newUserData = await _authService.RegisterAsync(registerRequest);

                if (newUserData == null)
                {
                    return BadRequest(new AppResponse<AuthResponse>
                    {
                        Success = false,
                        Message = "Email is already in use.",
                        ErrorCode = 400
                    });
                }

                return Ok(new AppResponse<AuthResponse>
                {
                    Success = true,
                    Data = newUserData,
                    Message = "Registration successful"
                });
            }
            catch (System.Exception ex)
            {
                var response = new AppResponse<AuthResponse>
                {
                    Success = false,
                    Message = "An error occurred during registration",
                    Error = ex.Message,
                    ErrorCode = 500
                };

                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Handles POST requests for password reset via email.
        /// </summary>
        /// <param name="email">The email of the user</param>
        /// <returns>Action result indicating success or failure</returns>
        [HttpPost("forgot-password")]
        public async Task<ActionResult<AppResponse<string>>> ForgotPassword(ForgotPasswordRequest emailRequest)
        {
            try
            {
                var isEmailSent = await _authService.SendResetPasswordEmailAsync(emailRequest.Email);
                if (!isEmailSent)
                {
                    return NotFound(new AppResponse<string>
                    {
                        Success = false,
                        Message = "Email not found",
                        ErrorCode = 404
                    });
                }

                return Ok(new AppResponse<string>
                {
                    Success = true,
                    Message = "Password reset email sent successfully"
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new AppResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while sending reset email",
                    Error = ex.Message,
                    ErrorCode = 500
                });
            }
        }

        /// <summary>
        /// Handles POST requests to reset the user's password.
        /// </summary>
        /// <param name="resetRequest">The reset password request object</param>
        /// <returns>Action result indicating success or failure</returns>
        [HttpPost("reset-password")]
        public async Task<ActionResult<AppResponse<string>>> ResetPassword(ResetPasswordRequest resetRequest)
        {
            try
            {
                var isReset = await _authService.ResetPasswordAsync(resetRequest);
                if (!isReset)
                {
                    return BadRequest(new AppResponse<string>
                    {
                        Success = false,
                        Message = "Invalid token or password reset failed",
                        ErrorCode = 400
                    });
                }

                return Ok(new AppResponse<string>
                {
                    Success = true,
                    Message = "Password reset successful"
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new AppResponse<string>
                {
                    Success = false,
                    Message = "An error occurred during password reset",
                    Error = ex.Message,
                    ErrorCode = 500
                });
            }
        }

        /// <summary>
        /// Handles POST requests to change the user's password.
        /// </summary>
        /// <param name="changePasswordRequest">The change password request object</param>
        /// <returns>Action result indicating success or failure</returns>
        [HttpPost("change-password")]
        public async Task<ActionResult<AppResponse<string>>> ChangePassword(ChangePasswordRequest changePasswordRequest)
        {
            try
            {
                var isChanged = await _authService.ChangePasswordAsync(changePasswordRequest);
                if (!isChanged)
                {
                    return BadRequest(new AppResponse<string>
                    {
                        Success = false,
                        Message = "Password change failed",
                        ErrorCode = 400
                    });
                }

                return Ok(new AppResponse<string>
                {
                    Success = true,
                    Message = "Password changed successfully"
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new AppResponse<string>
                {
                    Success = false,
                    Message = "An error occurred during password change",
                    Error = ex.Message,
                    ErrorCode = 500
                });
            }
        }
    }
}

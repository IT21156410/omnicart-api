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
using omnicart_api.Requests;
using omnicart_api.Services;
using System.IdentityModel.Tokens.Jwt;

namespace omnicart_api.Controllers
{
    [Route("api")]
    [ApiController]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly UserService _userService;
        private readonly string _adminToken;

        /// <summary>
        /// Initializes the AuthController with AuthService dependency.
        /// </summary>
        /// <param name="authService">The authentication service</param>
        public AuthController(AuthService authService, UserService userService)
        {
            _authService = authService;
            _userService = userService;
            _adminToken = "ApilageAdmin@2024*RataAnurata*Mama#OSMA"; // TODO: get from env
        }

        /// <summary>
        /// Handles GET requests to retrieve the authenticated user's own data.
        /// </summary>
        /// <returns>The authenticated user's data</returns>
        [HttpGet("own-user")]
        public async Task<ActionResult<AppResponse<UserDto>>> GetOwnUser()
        {
            try
            {
                // user ID from the JWT token (retrieved from the authenticated context)
                var userId = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

                if (userId == null)
                {
                    return Unauthorized(new AppResponse<UserDto>
                    {
                        Success = false,
                        Message = "User is not authenticated",
                        ErrorCode = 401
                    });
                }

                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new AppResponse<UserDto>
                    {
                        Success = false,
                        Message = "User not found",
                        ErrorCode = 403
                    });
                }

                return Ok(new AppResponse<UserDto>
                {
                    Success = true,
                    Data = new UserDto(user),
                    Message = "User data retrieved successfully"
                });
            }
            catch (System.Exception ex)
            {
                var response = new AppResponse<UserDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user data",
                    Error = ex.Message,
                    ErrorCode = 500
                };

                return StatusCode(500, response);
            }
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
                if (registerRequest.Role == "admin" && (registerRequest.AdminToken != null && registerRequest.AdminToken != _adminToken))
                {
                    return Unauthorized(new AppResponse<AuthResponse>
                    {
                        Success = false,
                        Message = "Invalid admin token, contact super admin",
                        ErrorCode = 401
                    });
                }

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
                        ErrorCode = 403
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
                        ErrorCode = 401
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
                        ErrorCode = 401
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

        /// <summary>
        /// Handles POST requests to verify the user's two-factor authentication (2FA) code.
        /// </summary>
        /// <param name="verifyTwoFactorRequest">The request object containing the 2FA code and user information</param>
        /// <returns>Action result indicating success or failure of the 2FA verification</returns>
        [HttpPost("verify-2fa")]
        public async Task<ActionResult<AppResponse<string>>> VerifyTwoFactor(VerifyTwoFactorRequest verifyTwoFactorRequest)
        {
            try
            {
                var isVerified = await _authService.VerifyTwoFactorAsync(verifyTwoFactorRequest);
                if (!isVerified)
                {
                    return BadRequest(new AppResponse<string>
                    {
                        Success = false,
                        Message = "Invalid 2FA code",
                        ErrorCode = 401
                    });
                }

                return Ok(new AppResponse<string>
                {
                    Success = true,
                    Message = "Two factor authentication successful"
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new AppResponse<string>
                {
                    Success = false,
                    Message = "An error occurred during 2FA verification",
                    Error = ex.Message,
                    ErrorCode = 500
                });
            }
        }

        /// <summary>
        /// Handles POST requests to send the user's two-factor authentication (2FA) code.
        /// </summary>
        /// <param name="sendTwoFactorRequest">The request object containing the user's email</param>
        /// <returns>Action result indicating success or failure of sending the 2FA code</returns>
        [HttpPost("send-2fa-verify")]
        public async Task<ActionResult<AppResponse<string>>> SendTwoFactorVerify(SendTwoFactorRequest sendTwoFactorRequest)
        {
            try
            {
                var user = await _userService.FindByEmailAsync(sendTwoFactorRequest.Email);
                if (user == null)
                {
                    return NotFound(new AppResponse<string>
                    {
                        Success = false,
                        Message = "User not found",
                        ErrorCode = 404
                    });
                }

                var code = await _authService.Generate2FAVerifyTokenAsync(user);
                if (code == null)
                {
                    return NotFound(new AppResponse<string>
                    {
                        Success = false,
                        Message = "Code not found, try again",
                        ErrorCode = 403
                    });
                }

                //var emailService = new EmailService();
                //await emailService.SendTwoFactorCodeAsync(user.Email, code);

                return Ok(new AppResponse<string>
                {
                    Success = true,
                    Message = "2FA verification code sent successfully"
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new AppResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while sending the 2FA code",
                    Error = ex.Message,
                    ErrorCode = 500
                });
            }
        }

    }
}

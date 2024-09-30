
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

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AppResponse<string>>> Login(LoginRequest loginRequest)
        {
            try
            {
                var token = await _authService.LoginAsync(loginRequest);
                if (token == null)
                {
                    return Unauthorized(new AppResponse<string>
                    {
                        Success = false,
                        Message = "Invalid credentials",
                        ErrorCode = 401
                    });
                }

                return Ok(new AppResponse<string>
                {
                    Success = true,
                    Data = token,
                    Message = "Login successful"
                });
            }
            catch (System.Exception ex)
            {
                var response = new AppResponse<string>
                {
                    Success = false,
                    Message = "An error occurred during login",
                    Error = ex.Message,
                    ErrorCode = 500
                };

                return StatusCode(500, response);
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<AppResponse<User>>> Register(RegisterRequest registerRequest)
        {
            try
            {
                var newUser = await _authService.RegisterAsync(registerRequest);

                if (newUser == null)
                {
                    return BadRequest(new AppResponse<User>
                    {
                        Success = false,
                        Message = "Email is already in use.",
                        ErrorCode = 400
                    });
                }

                return Ok(new AppResponse<User>
                {
                    Success = true,
                    Data = newUser,
                    Message = "Registration successful"
                });
            }
            catch (System.Exception ex)
            {
                var response = new AppResponse<User>
                {
                    Success = false,
                    Message = "An error occurred during registration",
                    Error = ex.Message,
                    ErrorCode = 500
                };

                return StatusCode(500, response);
            }
        }

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

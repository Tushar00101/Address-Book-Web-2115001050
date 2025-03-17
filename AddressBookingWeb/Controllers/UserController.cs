using BusinessLayer.Interface;
using JWT.Service;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Model;
using NLog;
using Middleware.GlobalExceptionHandler;
using RabbitProducer.Service;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AddressBookApplication.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IUserBL _userBL;
        private readonly TokenService _jwtService;
        private readonly RabbitMqProducer _rabbitMQProducer;

        public UserController(IUserBL userBL, TokenService jwtService, RabbitMqProducer rabbitMQProducer)
        {
            _userBL = userBL;
            _jwtService = jwtService;
            _rabbitMQProducer = rabbitMQProducer;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserDTO userDTO)
        {
            if (!ModelState.IsValid)
            {
                logger.Warn("Validation error in Register request.");
                return BadRequest(ModelState);
            }

            try
            {
                logger.Info("User registration process started.");
                var result = _userBL.RegisterBL(userDTO);

                if (result == null)
                {
                    logger.Warn("User already exists.");
                    return Ok(new ResponseModel<object>
                    {
                        Success = false,
                        Message = "User already present",
                        Data = null
                    });
                }

                logger.Info("User registered successfully.");
                var message = new
                {
                    To = userDTO.Email,
                    Subject = "Registration",
                    Body = "You registered successfully"
                };

                _rabbitMQProducer.PublishMessage(message);
                return Ok(new ResponseModel<object>
                {
                    Success = true,
                    Message = "User registered",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception in Register");
                return BadRequest(ExceptionHandler.CreateErrorResponse(ex));
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                logger.Warn("Validation error in Login request.");
                return BadRequest(ModelState);
            }

            try
            {
                logger.Info("User login attempt started.");
                var result = _userBL.LoginBL(loginDTO);

                if (result == null)
                {
                    logger.Warn("Invalid login credentials.");
                    return Ok(new ResponseModel<object>
                    {
                        Success = false,
                        Message = "Invalid email or password",
                        Data = null
                    });
                }

                string token = _jwtService.GenerateToken(result.Id, loginDTO.Email,result.Role);
                result.Token = token;

                logger.Info("User logged in successfully.");
                return Ok(new ResponseModel<ResponseLoginDTO>
                {
                    Success = true,
                    Message = "User login successful",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception in Login");
                return BadRequest(ExceptionHandler.CreateErrorResponse(ex));
            }
        }






        /// <summary>
        /// Sends a password reset email to the user.
        /// </summary>
        [HttpPost("forget")]
        public async Task<IActionResult> Forget([FromBody] ForgetDTO forgetDTO)
        {
            if (!ModelState.IsValid)
            {
                logger.Warn("Validation error in Forget request.");
                return BadRequest(ModelState);
            }

            try
            {
                logger.Info($"Password reset request received for email: {forgetDTO.Email}");

                var result = _userBL.ForgetBL(forgetDTO);

                if (!result)
                {
                    logger.Warn("User not found for password reset.");
                    return Ok(new ResponseModel<object>
                    {
                        Success = false,
                        Message = "No user found with this email",
                        Data = null
                    });
                }

                string resetToken = _jwtService.GenerateResetToken(forgetDTO.Email);
                string resetLink = $"https://localhost:7277/api/user/reset?token={resetToken}";

                var message = new
                {
                    To = forgetDTO.Email,
                    Subject = "Reset Your Password",
                    Body = $"Click the link to reset your password: https://localhost:7277/api/user/reset-password?token={resetToken}"
                };

                //await _emailService.SendEmailAsync(forgetDTO.Email, "Reset Your Password",
                //    $"Click the link to reset your password: <a href='{resetLink}'>Reset Password</a>");

                //logger.Info("Password reset email sent successfully.");
                _rabbitMQProducer.PublishMessage(message);
                return Ok(new ResponseModel<object>
                {
                    Success = true,
                    Message = "Password reset email sent successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception in Forget");
                return BadRequest(ExceptionHandler.CreateErrorResponse(ex));
            }
        }

        /// <summary>
        /// Resets the user's password after verification.
        /// </summary>
        [HttpPost("reset-password")]
        public IActionResult Reset([FromBody] ResetDTO resetDTO)
        {
            try
            {
                string token = HttpContext.Request.Query["token"];
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(resetDTO.Password))
                {
                    return BadRequest(new ResponseModel<Object> { Success = true, Message = "Token and new password are required." });
                }

                var email = _jwtService.ValidateResetToken(token);
                if (email == null)
                {
                    return Unauthorized(new ResponseModel<Object> { Success=false,Message = "Invalid or expired token." });
                }

                bool updateSuccess = _userBL.UpdateUserPassword(email, resetDTO);
                if (!updateSuccess)
                {
                    return BadRequest(new ResponseModel<Object> { Success=false,Message = "Failed to update password." });
                }

                logger.Info("Password changed successfully.");
                return Ok(new ResponseModel<Object> { Success=true,Message = "Password changed successfully!" });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception in Reset");
                return BadRequest(ExceptionHandler.CreateErrorResponse(ex));
            }
        }



        /// <summary>
        /// Getting profile of current user
        /// </summary>

        [Authorize]
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            try
            {
                int userId = GetUserId();
                logger.Info($"Fetching profile for UserId: {userId}");

                var user = _userBL.GetUserProfile(userId);
                if (user == null)
                {
                    logger.Warn($"User profile not found for UserId: {userId}");
                    return NotFound(new ResponseModel<object>
                    {
                        Success = false,
                        Message = "User profile not found",
                        Data = null
                    });
                }

                // Return only relevant details (excluding password)
                var profileData = new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email
                };

                logger.Info($"Profile retrieved successfully for UserId: {userId}");
                return Ok(new ResponseModel<object>
                {
                    Success = true,
                    Message = "User profile retrieved",
                    Data = profileData
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception in GetProfile");
                return BadRequest(ExceptionHandler.CreateErrorResponse(ex));
            }
        }

        private int GetUserId()
        {
            try
            {
                var userIdClaims = User.Claims
                    .Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                    .Select(c => c.Value)
                    .ToList();

                if (!userIdClaims.Any())
                {
                    throw new Exception("User ID claim is missing in the JWT.");
                }

                foreach (var claim in userIdClaims)
                {
                    if (int.TryParse(claim, out int userId))
                    {
                        return userId;
                    }
                }
                throw new FormatException($"No valid integer User ID found in claims: {string.Join(", ", userIdClaims)}");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception in GetUserId");
                throw;
            }
        }


    }
}

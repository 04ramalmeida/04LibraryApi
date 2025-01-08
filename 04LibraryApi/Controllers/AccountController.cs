using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using _04LibraryApi.Data;
using _04LibraryApi.Data.Entities;
using _04LibraryApi.Data.Models;
using _04LibraryApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace _04LibraryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(IUserHelper userHelper,
        IConfiguration config,
        IMailHelper mailHelper,
        IBlobHelper blobHelper) : ControllerBase
    {
        
        
        
        
        [HttpPost("[action]")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await userHelper.GetUserAsync(username);
            if (user == null)
            {
                return BadRequest("Wrong username or password.");
            }
            var loginResult = await userHelper.LoginAsync(user, password);
            if (loginResult.Succeeded)
            {
                var key = config["Jwt:Secret"] ?? throw new ArgumentNullException("Jwt:Secret", "Jwt:Secret cannot be null.");
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(ClaimTypes.Email, user.Email!)
                };

                var token = new JwtSecurityToken
                (
                    issuer:  config["Jwt:Issuer"],
                    audience: config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddDays(10),
                    signingCredentials: credentials
                );
                
                var jwt = new JwtSecurityTokenHandler().WriteToken(token);
                
                return new ObjectResult(new
                {
                    AccessToken = jwt,
                    TokenType = "Bearer",
                    UserId = user.Id,
                    user.UserName
                });
            }

            return BadRequest("Wrong username or password.");
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Register(string username,
            string password,
            string firstname,
            string lastname)
        {
            User user = new User
            {
                UserName = username,
                Email = username,
                FirstName = firstname,
                LastName = lastname,
                CreatedOn = DateTime.Now,
                Library = new List<Book>(),
                ImageId = Guid.Empty
            };
            var result = await userHelper.CreateUserAsync(user, password);
            if (result != IdentityResult.Success)
            {
                return BadRequest(result.Errors.FirstOrDefault().Description);
                
            }

            string userToken = await userHelper.GenerateEmailConfirmationTokenAsync(user);
            Response response = mailHelper.SendEmail(username, "Email Confirmation",
                "To finish your registration, please enter the token \n " +
                $"{userToken}");

            if (response.IsSuccess)
            {
                return Ok("The user has been created.");
            }
            return BadRequest(response.Message);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ConfirmEmail([FromBody]EmailConfirmation confirmationInfo)
        {
            var user = await userHelper.GetUserAsync(confirmationInfo.Username);
            if (user == null)
            {
                return NotFound("Wrong username or token.");
            }
            var result = await userHelper.ConfirmEmailAsync(user, confirmationInfo.Token);
            if (result.Succeeded)
            {
                return Ok("Email confirmed.");
            }
            return BadRequest(result.Errors.FirstOrDefault().Description);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await userHelper.GetUserAsync(email);
            if (user == null)
            {
               return NotFound("No user found."); 
            }
            string token = await userHelper.GeneratePasswordResetTokenAsync(user);
            Response response = mailHelper.SendEmail(email, "Password Recovery",
                "To reset your password, enter this token on the app:\n " +
                $"{token}");

            if (response.IsSuccess)
            {
                return Ok("Check your email for further instructions.");
            }
            return BadRequest(response.Message);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> ResetPassword([FromBody]PasswordReset resetInfo)
        {
            var user = await userHelper.GetUserAsync(resetInfo.Email);
            if (user == null)
            {
                return BadRequest("Wrong email or token.");
            }
            
            var result = await userHelper.ResetPasswordAsync(user, resetInfo.Token, resetInfo.Password);

            if (result.Succeeded)
            {
                return Ok("Password reset successful.");
            }
            return BadRequest(result.Errors.FirstOrDefault().Description);
        }
        
        [Authorize]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetUserInfo()
        {
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                var username = identity.FindFirst(ClaimTypes.Email).Value;
                var userInfo = await userHelper.GetUserInfoAsync(username);
                return Ok(userInfo);
            }

            return Unauthorized();
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetUserPic()
        {
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                var username = identity.FindFirst(ClaimTypes.Email).Value;
                var user = await userHelper.GetUserAsync(username);
                return Ok(user.ImagePath);
            }
            return Unauthorized();  
        }
        
        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> UploadPic(IFormFile file)
        {
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                var username = identity.FindFirst(ClaimTypes.Email).Value;
                var user = await userHelper.GetUserAsync(username);
                try
                {
                    Guid imageId = await blobHelper.UploadBlobAsync(file);
                    user.ImageId = imageId;
                    var response = await userHelper.ChangeUserAsync(user);
                    if (response.Succeeded)
                    {
                        return Ok("The new picture has been set.");
                    }
                    return BadRequest(response.Errors.FirstOrDefault().Description);
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }
            return Unauthorized();
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> ChangeUserInfo([FromBody] UserInfo userInfo)
        {
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                var username = identity.FindFirst(ClaimTypes.Email).Value;
                var user = await userHelper.GetUserAsync(username);
                userInfo.FirstName = user.FirstName;
                userInfo.LastName = user.LastName; 
                var response = await userHelper.ChangeUserAsync(user);
                if (response.Succeeded)
                {
                    return Ok("Your information has been updated.");
                }
                return BadRequest(response.Errors.FirstOrDefault().Description);
            }
            return Unauthorized();  
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword changePassword)
        {
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                var username = identity.FindFirst(ClaimTypes.Email).Value;
                var user = await userHelper.GetUserAsync(username);
                var result = await userHelper.ChangePasswordAsync(user, changePassword.CurrentPassword, changePassword.NewPassword);
                if (result.Succeeded)
                {
                    return Ok("Your password has been updated.");
                }
                return BadRequest(result.Errors.FirstOrDefault().Description);
            }
            return Unauthorized();  
        }
    }
}

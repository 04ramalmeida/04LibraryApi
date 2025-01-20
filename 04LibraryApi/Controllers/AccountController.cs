using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using _04LibraryApi.Data;
using _04LibraryApi.Data.Entities;
using _04LibraryApi.Data.Models;
using _04LibraryApi.Helpers;
using _04LibraryApi.Repositories;
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
        IBlobHelper blobHelper,
        ILibraryRepository libraryRepository) : ControllerBase
    {
        
        
        
        
        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody]Login login)
        {
            var user = await userHelper.GetUserAsync(login.Username);
            if (user == null)
            {
                return BadRequest("Wrong username or password.");
            }
            var loginResult = await userHelper.LoginAsync(user, login.Password);
            if (loginResult.Succeeded)
            {
                var key = config["Jwt:Secret"] ?? throw new ArgumentNullException("Jwt:Secret", "Jwt:Secret cannot be null.");
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, userHelper.GetUserRole(user))
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
        public async Task<IActionResult> Register([FromBody]Register register)
        {
            User user = new User
            {
                UserName = register.Username,
                Email = register.Username,
                FirstName = register.FirstName,
                LastName = register.LastName,
                CreatedOn = DateTime.Now,
                ImageId = Guid.Empty
            };
            
            
            var result = await userHelper.CreateUserAsync(user, register.Password);
            if (result != IdentityResult.Success)
            {
                return BadRequest(result.Errors.FirstOrDefault().Description);
                
            }

            await userHelper.AddUserToRoleAsync(user, "User");
            
            Library library = new Library
            {
                UserId = user.Id
            };
            
            try
            {
                await libraryRepository.CreateAsync(library);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            
            string userToken = await userHelper.GenerateEmailConfirmationTokenAsync(user);
            MailResponse mailResponse = mailHelper.SendEmail(register.Username, "Email Confirmation",
                "To finish your registration, please enter the token \n " +
                $"{userToken}");

            if (mailResponse.IsSuccess)
            {
                return Ok("The user has been created.");
            }
            return BadRequest(mailResponse.Message);
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
            MailResponse mailResponse = mailHelper.SendEmail(email, "Password Recovery",
                "To reset your password, enter this token on the app:\n " +
                $"{token}");

            if (mailResponse.IsSuccess)
            {
                return Ok("Check your email for further instructions.");
            }
            return BadRequest(mailResponse.Message);
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
            AuthResponse authResponse = await userHelper.VerifyLogin(HttpContext.User.Identity);
            if (!authResponse.IsAuthorized)
            {
                return Unauthorized();
            }
            var userInfo = await userHelper.GetUserInfoAsync(authResponse.User.UserName);
            return Ok(userInfo);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetUserPic()
        {
            AuthResponse authResponse = await userHelper.VerifyLogin(HttpContext.User.Identity);
            if (!authResponse.IsAuthorized)
            {
                return Unauthorized();
            }

            return Ok(authResponse.User.ImagePath);
        }
        
        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> UploadPic(IFormFile file)
        {
            AuthResponse authResponse = await userHelper.VerifyLogin(HttpContext.User.Identity);
            if (!authResponse.IsAuthorized)
            {
                return Unauthorized();
            }

            try
            {
                Guid imageId = await blobHelper.UploadBlobAsync(file);
                authResponse.User.ImageId = imageId;
                var response = await userHelper.ChangeUserAsync(authResponse.User);
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

        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> ChangeUserInfo([FromBody] UserInfo userInfo)
        {
            var authResponse = await userHelper.VerifyLogin(HttpContext.User.Identity);
            if (!authResponse.IsAuthorized)
            {
                return Unauthorized();
            }
            authResponse.User.FirstName = userInfo.FirstName;
            authResponse.User.LastName = userInfo.LastName;
            var response = await userHelper.ChangeUserAsync(authResponse.User);
            if (response.Succeeded)
            {
                return Ok("Your information has been updated.");
            }
            return BadRequest(response.Errors.FirstOrDefault().Description);
             
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword changePassword)
        {
            AuthResponse authResponse = await userHelper.VerifyLogin(HttpContext.User.Identity);
            if (!authResponse.IsAuthorized)
            {
                return Unauthorized();
            }
            var result = await userHelper.ChangePasswordAsync(authResponse.User, changePassword.CurrentPassword, changePassword.NewPassword);
            if (result.Succeeded)
            {
                return Ok("Your password has been updated.");
            }
            return BadRequest(result.Errors.FirstOrDefault().Description);
              
        }
    }
}

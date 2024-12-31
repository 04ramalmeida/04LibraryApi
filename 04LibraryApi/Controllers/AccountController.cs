using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using _04LibraryApi.Data.Entities;
using _04LibraryApi.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace _04LibraryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(IUserHelper userHelper,
        IConfiguration config) : ControllerBase
    {
        [HttpPost("[action]")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await userHelper.GetUserAsync(username);
            if (user == null)
            {
                return NotFound("This user does not exist.");
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
            else
            {
                return BadRequest("Wrong username or password.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetUserInfo()
        {
            var username = string.Empty;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                username = identity.FindFirst(ClaimTypes.Email).Value;
            }
            if (username == null)
            {
                return Ok("nope");
            }
            else
            {
                return Ok(username);
            }
        }
    }
}

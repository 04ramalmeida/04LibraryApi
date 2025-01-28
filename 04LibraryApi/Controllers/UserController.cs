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
    public class UserController : ControllerBase
    {
        private readonly IUserHelper _userHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IConfiguration _config;
        private readonly IMailHelper _mailHelper;
        private readonly ILibraryRepository _libraryRepository;

        public UserController(IUserHelper userHelper,
            IConfiguration config,
            IMailHelper mailHelper,
            IBlobHelper blobHelper,
            ILibraryRepository libraryRepository)
        {
            _userHelper = userHelper;
            _config = config;
            _mailHelper = mailHelper;
            _blobHelper = blobHelper;
            _libraryRepository = libraryRepository;
        }


        [Authorize]
        [HttpGet("user-info")]
        public async Task<IActionResult> GetUserInfo()
        {
            AuthResponse authResponse = await _userHelper.VerifyLogin(HttpContext.User.Identity);
            if (!authResponse.IsAuthorized)
            {
                return Unauthorized();
            }
            var userInfo = await _userHelper.GetUserInfoAsync(authResponse.User.UserName);
            return Ok(userInfo);
        }

        [Authorize]
        [HttpGet("user-pic")]
        public async Task<IActionResult> GetUserPic()
        {
            AuthResponse authResponse = await _userHelper.VerifyLogin(HttpContext.User.Identity);
            if (!authResponse.IsAuthorized)
            {
                return Unauthorized();
            }

            return Ok(authResponse.User.ImagePath);
        }
        
        [Authorize]
        [HttpPut("user-pic")]
        public async Task<IActionResult> UploadPic(IFormFile file)
        {
            AuthResponse authResponse = await _userHelper.VerifyLogin(HttpContext.User.Identity);
            if (!authResponse.IsAuthorized)
            {
                return Unauthorized();
            }

            try
            {
                Guid imageId = await _blobHelper.UploadBlobAsync(file);
                authResponse.User.ImageId = imageId;
                var response = await _userHelper.ChangeUserAsync(authResponse.User);
                if (response.Succeeded)
                {
                    return Ok("The new picture has been set.");
                }
                return StatusCode(500, response.Errors.FirstOrDefault().Description);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
            
        }

        [Authorize]
        [HttpPut("user-info")]
        public async Task<IActionResult> ChangeUserInfo([FromBody] UserInfo userInfo)
        {
            var authResponse = await _userHelper.VerifyLogin(HttpContext.User.Identity);
            if (!authResponse.IsAuthorized)
            {
                return Unauthorized();
            }
            authResponse.User.FirstName = userInfo.FirstName;
            authResponse.User.LastName = userInfo.LastName;
            var response = await _userHelper.ChangeUserAsync(authResponse.User);
            if (response.Succeeded)
            {
                return Ok("Your information has been updated.");
            }
            return StatusCode(500, response.Errors.FirstOrDefault().Description);
             
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword changePassword)
        {
            AuthResponse authResponse = await _userHelper.VerifyLogin(HttpContext.User.Identity);
            if (!authResponse.IsAuthorized)
            {
                return Unauthorized();
            }
            var result = await _userHelper.ChangePasswordAsync(authResponse.User, changePassword.CurrentPassword, changePassword.NewPassword);
            if (result.Succeeded)
            {
                return Ok("Your password has been updated.");
            }
            return StatusCode(500, result.Errors.FirstOrDefault().Description);
              
        }
    }
}

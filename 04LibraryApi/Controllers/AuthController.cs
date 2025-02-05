using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using _04LibraryApi.Data;
using _04LibraryApi.Data.Entities;
using _04LibraryApi.Data.Models;
using _04LibraryApi.Helpers;
using _04LibraryApi.Repositories;
using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace _04LibraryApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
	private readonly IUserHelper _userHelper;
	private readonly IConfiguration _config;
	private readonly IMailHelper _mailHelper;
	private readonly ILibraryRepository _libraryRepository;


	
	public AuthController(IUserHelper userHelper,
	 IConfiguration config,
	  IMailHelper mailHelper,
	 ILibraryRepository libraryRepository)
	{
		_userHelper = userHelper;
		_config = config;
		_mailHelper = mailHelper;
		_libraryRepository = libraryRepository;
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody]LoginInfo loginInfo) 
	{
		var user = await _userHelper.GetUserAsync(loginInfo.Username);
		if (user == null)
		{
			return BadRequest("Wrong username or password.");
		}
		var loginResult = await _userHelper.LoginAsync(user, loginInfo.Password);
		if (loginResult.Succeeded)
		{
			var key = _config["Jwt:Secret"] ?? throw new ArgumentNullException("Jwt:Secret", "Jwt:Secret cannot be null.");
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
			
			var claims = new[]
			{
				new Claim(ClaimTypes.Email, user.Email),
				new Claim(ClaimTypes.Role, _userHelper.GetUserRole(user))	
			};
			
			var token = new JwtSecurityToken
			(
				issuer: _config["Jwt:Issuer"],
				audience: _config["Jwt:Audience"],
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
	
	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterInfo registerInfo)
	{
		User user = new User
		{
			UserName = registerInfo.Username,
			Email = registerInfo.Username,
			FirstName = registerInfo.FirstName,
			LastName = registerInfo.LastName,
			CreatedOn = DateTime.Now,
			ImageId = Guid.Empty
		};
		
		var result = await _userHelper.CreateUserAsync(user, registerInfo.Password);
		if (result != IdentityResult.Success)
		{
			return BadRequest(result.Errors.FirstOrDefault().Description);
		}

		await _userHelper.AddUserToRoleAsync(user, "User");
		
		Library library = new Library
		{
			UserId = user.Id,
		};

		try
		{
			await _libraryRepository.CreateAsync(library);
		}
		catch (Exception e)
		{
			return StatusCode(500, e.Message);
		}

		string userToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
		MailResponse mailResponse = _mailHelper.SendEmail(registerInfo.Username, "Email Confirmation",
			"To finish your registration, please enter the token \n " +
			$"{userToken}");

		if (mailResponse.IsSuccess)
		{
			return Ok("The user has been created.");
		}
		return StatusCode(500, mailResponse.Message);
	}

	[HttpPut("confirm")]
	public async Task<IActionResult> ConfirmEmail([FromBody]EmailConfirmation confirmation)
	{
		var user = await _userHelper.GetUserAsync(confirmation.Username);
		if (user == null)
		{
			return NotFound("Wrong username or token.");
		}
		var result = await _userHelper.ConfirmEmailAsync(user, confirmation.Token);
		if (result.Succeeded)
		{
			return Ok("Email confirmed");
		}
		return StatusCode(500, result.Errors.FirstOrDefault().Description);
	}

	[HttpPost("forgot-password")]
	public async Task<IActionResult> ForgotPassword([FromBody] string email)
	{
		var user = await _userHelper.GetUserAsync(email);
		if (user == null)
		{
			return NotFound("No user found.");
		}

		string token = await _userHelper.GeneratePasswordResetTokenAsync(user);
		MailResponse mailResponse = _mailHelper.SendEmail(email, "Password Recovery",
			"To reset your password, enter this token on the app:\n " +
			$"{token}");

		if (mailResponse.IsSuccess)
		{
			return Ok("Password reset");
		}
		return StatusCode(500, mailResponse.Message);
	}

	[HttpPut("reset-password")]
	public async Task<IActionResult> ResetPassword([FromBody] PasswordReset resetInfo)
	{
		var user = await _userHelper.GetUserAsync(resetInfo.Email);
		if (user == null)
		{
			return BadRequest("Wrong email or password.");
		}
		var result = await _userHelper.ResetPasswordAsync(user, resetInfo.Token, resetInfo.Password);

		if (result.Succeeded)
		{
			return Ok("Password reset succesful.");
		}
		
		return StatusCode(500, result.Errors.FirstOrDefault().Description);
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
			return Ok("Password changed");
		}
		return StatusCode(500, result.Errors.FirstOrDefault().Description);
	}
	
	[Authorize]
	[HttpGet]
	public async Task<IActionResult> VerifyLogin() 
	{
		AuthResponse authResponse = await _userHelper.VerifyLogin(HttpContext.User.Identity);
		if (!authResponse.IsAuthorized) return Unauthorized();
		return Ok("");

	}
}
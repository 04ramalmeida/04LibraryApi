using System.Security.Claims;
using _04LibraryApi.Data;
using _04LibraryApi.Data.Entities;
using _04LibraryApi.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace _04LibraryApi.Helpers;

public class UserHelper : IUserHelper
{
    private readonly DataContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<User> _signInManager;

    public UserHelper(DataContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<User> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
    }
    public async Task CheckRoleAsync(string roleName)
    {
        var roleExists = await _roleManager.RoleExistsAsync(roleName);

        if (!roleExists)
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
    
    
    public async Task<User> GetUserAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email); 
    }

    public async Task<IdentityResult> CreateUserAsync(User user, string password)
    {
        return await _userManager.CreateAsync(user, password);
    }

    public async Task<bool> IsInRoleAsync(User user, string roleName)
    {
        return await _userManager.IsInRoleAsync(user, roleName);
    }

    public async Task AddUserToRoleAsync(User user, string roleName)
    {
        await _userManager.AddToRoleAsync(user, roleName);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
    {
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
    {
        return await _userManager.ConfirmEmailAsync(user, token);
    }

    public async Task<SignInResult> LoginAsync(User user, string password)
    {
        return await _signInManager.PasswordSignInAsync(user, password, false, false);
    }

    public async Task<AuthResponse> VerifyLogin(object auth)
    {
        AuthResponse result = new AuthResponse();
        if (auth is ClaimsIdentity identity)
        {
            var email = identity.FindFirst(ClaimTypes.Email).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                result.User = user;
                result.IsAuthorized = true;
            }
        }
        return result;
    }
    
    public async Task<PrivateUserInfo> GetPrivateUserInfoAsync(string userName)
    {
        var user = await GetUserFromUserNameAsync(userName);
        var userInfo = new PrivateUserInfo
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.UserName,
            Email = user.Email,
            CreatedOn = user.CreatedOn
        };
        return userInfo;
    }

    public async Task<PublicUserInfo> GetPublicUserInfoAsync(string userName)
    {
        var user = await GetUserFromUserNameAsync(userName);
        var userInfo = new PublicUserInfo
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.UserName,
            CreatedOn = user.CreatedOn
        };
        return userInfo;
    }
    
    public async Task<IdentityResult> RegisterUserAsync(User user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        return result;
    }

    public async Task<string> GeneratePasswordResetTokenAsync(User user)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
    {
        return await _userManager.ResetPasswordAsync(user, token, newPassword);
    }

    public async Task<IdentityResult> ChangeUserAsync(User user)
    {
        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
    {
        return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public string GetUserRole(User user)
    {
        return _userManager.GetRolesAsync(user).Result.FirstOrDefault();
    }

    public Task<User> GetUserFromUserNameAsync(string userName)
    {
        return _userManager.FindByNameAsync(userName);
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
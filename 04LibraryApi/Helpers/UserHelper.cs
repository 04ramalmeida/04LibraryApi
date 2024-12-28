using _04LibraryApi.Data;
using _04LibraryApi.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace _04LibraryApi.Helpers;

public class UserHelper : IUserHelper
{
    private readonly DataContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserHelper(DataContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }
    public async Task CheckRoleAsync(string roleName)
    {
        var roleExists = await _roleManager.RoleExistsAsync(roleName);

        if (!roleExists)
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    public async Task<User> GetUserAsync(string userName)
    {
        return await _userManager.FindByEmailAsync(userName); 
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

    public async Task ConfirmEmailAsync(User user, string token)
    {
        await _userManager.ConfirmEmailAsync(user, token);
    }
}
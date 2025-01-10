using _04LibraryApi.Data.Entities;
using _04LibraryApi.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace _04LibraryApi.Helpers;

public interface IUserHelper
{
    Task CheckRoleAsync(string roleName);
    
    Task<User> GetUserAsync(string userName);
    
    Task<IdentityResult> CreateUserAsync(User user, string password);
    
    Task<bool> IsInRoleAsync(User user, string roleName);
    
    Task AddUserToRoleAsync(User user, string roleName);
    
    Task<string> GenerateEmailConfirmationTokenAsync(User user);
    
    Task<IdentityResult> ConfirmEmailAsync(User user, string token);
    
   Task<SignInResult> LoginAsync(User user, string password);

   Task<UserInfo> GetUserInfoAsync(string userName);

   Task<string> GeneratePasswordResetTokenAsync(User user);

   Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword);

   Task<IdentityResult> ChangeUserAsync(User user);

   Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword);

   string GetUserRole(User user);
}
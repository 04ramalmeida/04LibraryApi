using _04LibraryApi.Data.Entities;
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
    
    Task ConfirmEmailAsync(User user, string token);
    
   Task<SignInResult> LoginAsync(User user, string password); 
}
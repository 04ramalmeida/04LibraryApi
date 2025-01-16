using _04LibraryApi.Data.Entities;
using _04LibraryApi.Helpers;
using _04LibraryApi.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace _04LibraryApi.Data;

public class DataSeed
{
    private readonly DataContext _context;
    private readonly IUserHelper _userHelper;
    private readonly ILibraryRepository _libraryRepository;
    
    public DataSeed(DataContext context,
        IUserHelper userHelper,
        ILibraryRepository libraryRepository)
    {
        _context = context;
        _userHelper = userHelper;
        _libraryRepository = libraryRepository;
    }

    public async Task InitSeedAsync()
    {
        await _context.Database.MigrateAsync();
        await InitRolesAsync();
        await CreateLibrarianAsync();
        await CreateAdminAsync();
    }

    private async Task InitRolesAsync()
    {
        await _userHelper.CheckRoleAsync("Admin");
        await _userHelper.CheckRoleAsync("Librarian");
        await _userHelper.CheckRoleAsync("User");
    }

    private async Task CreateLibrarianAsync()
    {
        var user = await _userHelper.GetUserAsync("masterlibrarian@library.com");

        if (user == null)
        {
            user = new User
            {
                FirstName = "Master",
                LastName = "Librarian",
                Email = "masterlibrarian@library.com",
                UserName = "masterlibrarian@library.com",
                PhoneNumber = "123456789",
                CreatedOn = DateTime.Now,
                ImageId = Guid.Empty
            };

            var result = await _userHelper.CreateUserAsync(user, "1234567");

            if (result != IdentityResult.Success)
            {
                throw new InvalidOperationException("Failed to create user");
            }
            
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
                throw new Exception(e.Message);
            }
        }
        
        

        var isInRole = await _userHelper.IsInRoleAsync(user, "Librarian");
        
        if (!isInRole)
        {
            await _userHelper.AddUserToRoleAsync(user, "Librarian");
        }
        
        var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
        await _userHelper.ConfirmEmailAsync(user, token);
        
    }
    
    private async Task CreateAdminAsync()
    {
        var user = await _userHelper.GetUserAsync("admin@library.com");

        if (user == null)
        {
            user = new User
            {
                FirstName = "Admin",
                LastName = "Library",
                Email = "admin@library.com",
                UserName = "admin@library.com",
                PhoneNumber = "123456789",
                CreatedOn = DateTime.Now,
                ImageId = Guid.Empty
            };

            var result = await _userHelper.CreateUserAsync(user, "1234567");

            if (result != IdentityResult.Success)
            {
                throw new InvalidOperationException("Failed to create user");
            }
        }

        var isInRole = await _userHelper.IsInRoleAsync(user, "Admin");
        
        if (!isInRole)
        {
            await _userHelper.AddUserToRoleAsync(user, "Admin");
        }
        
        var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
        await _userHelper.ConfirmEmailAsync(user, token);
        
    }
}
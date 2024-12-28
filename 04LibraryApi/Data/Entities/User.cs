using Microsoft.AspNetCore.Identity;

namespace _04LibraryApi.Data.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public DateTime CreatedOn { get; set; }
    
    public string? ImageUrl { get; set; }
    
    public ICollection<Book> Library { get; set; }    
}
using System.Collections;
using Microsoft.AspNetCore.Identity;

namespace _04LibraryApi.Data.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public DateTime CreatedOn { get; set; }
    
    public Guid? ImageId { get; set; }

    public string? ImagePath => ImageId == Guid.Empty
        ? "https://jalmaquablob.blob.core.windows.net/library-avatars/default.png"
        : $"https://jalmaquablob.blob.core.windows.net/library-avatars/{ImageId}";
    
    public ICollection<Book> Library { get; set; } 
}
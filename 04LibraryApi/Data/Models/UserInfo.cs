using _04LibraryApi.Data.Entities;

namespace _04LibraryApi.Data.Models;

public class UserInfo
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }

    public string? Email { get; set; }
    
    public DateTime? CreatedOn { get; set; }
    
}
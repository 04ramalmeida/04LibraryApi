namespace _04LibraryApi.Data.Models;

public class PrivateUserInfo
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string UserName { get; set; }
    
    public string? Email { get; set; }
    
    public DateTime? CreatedOn { get; set; }
}
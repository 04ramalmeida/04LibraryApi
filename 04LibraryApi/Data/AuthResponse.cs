using _04LibraryApi.Data.Entities;

namespace _04LibraryApi.Data;

public class AuthResponse
{
    public bool IsAuthorized { get; set; }
    
    public User? User { get; set; }
}
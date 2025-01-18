namespace _04LibraryApi.Data.Entities;

public class LibraryEntry : IEntity
{
    public int Id { get; set; }
    
    public int LibraryId { get; set; }
    
    public int BookId { get; set; }
    
    public bool HasRead { get; set; }
}
namespace _04LibraryApi.Data.Entities;

public class Library : IEntity
{
    public int Id { get; set; }
    public required string UserId { get; set; }

    public ICollection<LibraryEntry> Books { get; set; } = new List<LibraryEntry>();
}
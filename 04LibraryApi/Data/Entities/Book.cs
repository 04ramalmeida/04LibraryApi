namespace _04LibraryApi.Data.Entities;

public class Book : IEntity
{
    public int Id { get; set; }

    public string Name { get; set; }
    
    public string Author { get; set; }

    public string Genre { get; set; }
    
    public string Publisher { get; set; }
    
    public DateTime PublishDate { get; set; }
    
    public string? ImageURL { get; set; }
}
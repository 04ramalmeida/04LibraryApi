namespace _04LibraryApi.Data.Models;

public class BookInfo
{
    public string Name { get; set; }
    
    public string Author { get; set; }

    public string Genre { get; set; }
    
    public string Publisher { get; set; }
    
    public string PublishDate { get; set; }
    
    public string? ImageURL { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace _04LibraryApi.Data.Entities;

public class LibraryEntry : IEntity
{
    public int Id { get; set; }
    
    public int LibraryId { get; set; }
    
    public int BookId { get; set; }
    
    public bool HasRead { get; set; }

    private int _rating;
    
    public int Rating
    {
        get
        {
            return _rating;
        }

        set
        {
            if (value < 0) _rating = 0;
            else if (value > 5) _rating = 5;
            else _rating = value;

        }
    }
    
    [MaxLength(260)]
    public string? Review {get; set;}
}
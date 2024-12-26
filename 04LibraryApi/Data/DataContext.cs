using _04LibraryApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace _04LibraryApi.Data;

public class DataContext: DbContext
{
    
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    }
    public DbSet<Book> Books { get; set; }
    
}
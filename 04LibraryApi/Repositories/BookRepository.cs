using _04LibraryApi.Data;
using _04LibraryApi.Data.Entities;

namespace _04LibraryApi.Repositories;

public class BookRepository: GenericRepository<Book>, IBookRepository
{
    private readonly DataContext _context;
    
    public BookRepository(DataContext context) : base(context)
    {
        _context = context;
    }
    
    
}
using _04LibraryApi.Data;
using _04LibraryApi.Data.Entities;

namespace _04LibraryApi.Repositories;

public class LibraryRepository: GenericRepository<Library>, ILibraryRepository
{
    
    
    
    public LibraryRepository(DataContext context) : base(context)
    {
    }
}
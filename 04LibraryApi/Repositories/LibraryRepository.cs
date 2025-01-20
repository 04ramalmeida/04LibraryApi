using _04LibraryApi.Data;
using _04LibraryApi.Data.Entities;
using Microsoft.EntityFrameworkCore;


namespace _04LibraryApi.Repositories;

public class LibraryRepository: GenericRepository<Library>, ILibraryRepository
{
    private readonly DataContext _context;
    
    public LibraryRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Library> GetLibraryByUserId(string userId)
    {
        return await _context.Libraries.FirstOrDefaultAsync(l => l.UserId == userId);
    }
    
    public async Task<IEnumerable<Book>> GetLibraryEntriesByUserIdAsync(string userId)
    {
        var libraryId = await _context.Libraries
            .Where(l => l.UserId == userId)
            .Select(l => l.Id)
            .FirstOrDefaultAsync();

        var libraryEntries = await _context.LibraryEntries
            .Where(e => e.LibraryId == libraryId)
            .ToListAsync();
        
        return AggregateBooksFromEntries(libraryEntries);

    }

    public async Task<Book> GetBookByEntryId(int entryId)
    {
        var entry = await GetEntryById(entryId);
        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == entry.BookId);
        return book;
    }
    
    public async Task AddLibraryEntry(LibraryEntry libraryEntry, int libraryId)
    {
        var library = await _context.Libraries.FirstOrDefaultAsync(l => l.Id == libraryId);
        library.Books.Add(libraryEntry);
        await UpdateAsync(library);
    }

    public async Task DeleteLibraryEntry(int entryId)
    {
        var libraryEntry = await GetEntryById(entryId);
        _context.LibraryEntries.Remove(libraryEntry);
        await _context.SaveChangesAsync();
    }

    private ICollection<Book> AggregateBooksFromEntries(List<LibraryEntry> libraryEntries)
    {
        var result = new List<Book>();
        libraryEntries.ForEach( le =>
        {
            var currBook = _context.Books.FirstOrDefault(b => b.Id == le.BookId);
            if (currBook != null) result.Add(currBook);
        });
        return result;
    }

    public async Task<LibraryEntry> GetEntryById(int entryId)
    {
        return await _context.LibraryEntries.FirstOrDefaultAsync(l => l.Id == entryId);
    }

    public async Task SetHasRead(int entryId)
    {
        var libraryEntry = await GetEntryById(entryId);
        libraryEntry.HasRead = libraryEntry.HasRead == false;
        _context.LibraryEntries.Update(libraryEntry);
        await _context.SaveChangesAsync();
    }

    public async Task<Library> GetLibraryByEntryId(int entryId)
    {
        var entry = await GetEntryById(entryId);
        return await _context.Libraries.FirstOrDefaultAsync(l => l.Id == entry.LibraryId);
    }
}
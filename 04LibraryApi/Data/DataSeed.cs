using Microsoft.EntityFrameworkCore;

namespace _04LibraryApi.Data;

public class DataSeed
{
    private readonly DataContext _context;

    public DataSeed(DataContext context)
    {
        _context = context;
    }

    public async Task InitSeedAsync()
    {
        await _context.Database.MigrateAsync();
    }
}
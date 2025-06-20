﻿using _04LibraryApi.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace _04LibraryApi.Data;

public class DataContext: IdentityDbContext<User>
{
    
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    }
    public DbSet<Book> Books { get; set; }
    
    public DbSet<Library> Libraries { get; set; }
    
    public DbSet<LibraryEntry> LibraryEntries { get; set; }
    
    public DbSet<ExpiredToken> ExpiredTokens { get; set; }
    
}
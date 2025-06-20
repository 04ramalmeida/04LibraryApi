﻿using _04LibraryApi.Data.Entities;

namespace _04LibraryApi.Repositories;

public interface ILibraryRepository : IGenericRepository<Library>
{
    Task<IEnumerable<LibraryEntry>> GetLibraryEntriesByUserIdAsync(string userId);

    Task<Library> GetLibraryByUserId(string userId);

    Task AddLibraryEntry(LibraryEntry libraryEntry, int libraryId);

    Task<Book> GetBookByEntryId(int entryId);

    Task<LibraryEntry> GetEntryById(int entryId);
    
    Task DeleteLibraryEntry(int entryId);

    Task SetHasRead(int entryId);

    Task<Library> GetLibraryByEntryId(int entryId);
    
    Task<bool> VerifyBookAlreadyHasEntry(int bookId, string userId);

    Task SetRating(int entryId, int rating);
    
    Task SetReview(int entryId, string review);
}
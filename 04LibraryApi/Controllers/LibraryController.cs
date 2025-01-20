using System.Security.Claims;
using _04LibraryApi.Data;
using _04LibraryApi.Data.Entities;
using _04LibraryApi.Helpers;
using _04LibraryApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _04LibraryApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LibraryController : ControllerBase
{
    private readonly IUserHelper _userHelper;
    private readonly ILibraryRepository _libraryRepository;
    private readonly IBookRepository _bookRepository;
    
    public LibraryController(ILibraryRepository libraryRepository
        , IUserHelper userHelper,
        IBookRepository bookRepository)
    {
        _libraryRepository = libraryRepository;
        _userHelper = userHelper;
        _bookRepository = bookRepository;
    }

    [Authorize]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetLibraryEntries()
    {
        AuthResponse authResponse = await _userHelper.VerifyLogin(HttpContext.User.Identity);
        if (!authResponse.IsAuthorized)
        {
            return Unauthorized();
        }
        try
        {
            var libraryEntries = await _libraryRepository.GetLibraryEntriesByUserIdAsync(authResponse.User.Id);
            if (!libraryEntries.Any())
            {
                return NotFound();
            }
            return Ok(libraryEntries);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        
    }

    [Authorize]
    [HttpPost("[action]")]
    public async Task<IActionResult> AddLibraryEntry(int bookId)
    {
        AuthResponse authResponse = await _userHelper.VerifyLogin(HttpContext.User.Identity);
        if (!authResponse.IsAuthorized)
        {
            return Unauthorized();
        }
        var library = await _libraryRepository.GetLibraryByUserId(authResponse.User.Id);
        if (library == null)
        {
            return Unauthorized();
        }
        
        var book = await _bookRepository.GetByIdAsync(bookId);
        
        if (book == null)
        {
            return NotFound("Book not found");
        }
            
        LibraryEntry newEntry = new LibraryEntry
        {
            BookId = bookId,
            LibraryId = library.Id
        };

        try
        {
            await _libraryRepository.AddLibraryEntry(newEntry, library.Id);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        return Ok();
    }

    [Authorize]
    [HttpPost("[action]")]
    public async Task<IActionResult> RemoveLibraryEntry(int entryId)
    {
        //TODO: Verify user ownership of entry
        var entry = await _libraryRepository.GetEntryById(entryId);
        if (entry == null)
        {
            return NotFound();
        }

        try
        {
            await _libraryRepository.DeleteLibraryEntry(entryId);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        return Ok();
    }

    [Authorize]
    [HttpPost("[action]")]
    public async Task<IActionResult> SetEntryReadStatus(int entryId)
    {
        //TODO: Verify user ownership of entry
        var entry = await _libraryRepository.GetEntryById(entryId);
        if (entry == null)
        {
            return NotFound();
        }

        try
        {
            await _libraryRepository.SetHasRead(entryId);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        return Ok();
    }
}
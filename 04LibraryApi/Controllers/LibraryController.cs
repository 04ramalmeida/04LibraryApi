using System.ComponentModel.DataAnnotations;
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

    //Change to return more than just the book, it currently doesn't return the reading status
    [Authorize]
    [HttpGet("entries")]
    public async Task<IActionResult> GetLibraryEntries(int? entryId)
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

            if (entryId != null)
            {
                var result = libraryEntries.FirstOrDefault(le => le.Id == entryId);
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            return Ok(libraryEntries);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
        
    }

    
    [Authorize]
    [HttpPost("entries")]
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

        var bookExists = await _libraryRepository.VerifyBookAlreadyHasEntry(bookId, authResponse.User.Id);
        
        if (bookExists) return BadRequest("Book already exists");
        
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
            return StatusCode(500, e.Message);
        }
        return Ok();
    }

    [Authorize]
    [HttpDelete("entries")]
    public async Task<IActionResult> RemoveLibraryEntry(int entryId)
    {
        AuthResponse authResponse = await _userHelper.VerifyLogin(HttpContext.User.Identity);
        var entry = await _libraryRepository.GetEntryById(entryId);
        
        if (entry == null)
        {
            return NotFound();
        }

        var library = await _libraryRepository.GetLibraryByEntryId(entryId);
        
        if (!authResponse.IsAuthorized || authResponse.User.Id != library.UserId)
        {
            return Unauthorized();
        }
        
        try
        {
            await _libraryRepository.DeleteLibraryEntry(entryId);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
        return Ok();
    }

    [Authorize]
    [HttpPut("entries/status")]
    public async Task<IActionResult> SetEntryReadStatus(int entryId)
    {
        AuthResponse authResponse = await _userHelper.VerifyLogin(HttpContext.User.Identity);
        var entry = await _libraryRepository.GetEntryById(entryId);
        
        if (entry == null)
        {
            return NotFound();
        }

        var library = await _libraryRepository.GetLibraryByEntryId(entryId);
        
        if (!authResponse.IsAuthorized || authResponse.User.Id != library.UserId)
        {
            return Unauthorized();
        }
        

        try
        {
            await _libraryRepository.SetHasRead(entryId);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
        return Ok();
    }

    [Authorize]
    [HttpPut("entries/rating")]
    public async Task<IActionResult> SetRating(int entryId, int rating)
    {

        if (rating < 1 || rating > 5)
        {
            return BadRequest("The rating must be between 1 and 5.");
        }
        
        AuthResponse authResponse = await _userHelper.VerifyLogin(HttpContext.User.Identity);
        var entry = await _libraryRepository.GetEntryById(entryId);

        if (entry == null)
        {
            return NotFound();
        }
        
        var library = await _libraryRepository.GetLibraryByEntryId(entryId);
        
        if (!authResponse.IsAuthorized || authResponse.User.Id != library.UserId)
        {
            return Unauthorized();
        }

        try
        {
            await _libraryRepository.SetRating(entryId, rating);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
        return Ok();
    }

    [Authorize]
    [HttpPut("entries/review")]
    public async Task<IActionResult> SetReview(int entryId, [FromBody]string review)
    {
        if (review.Length > 260)
        {
            return BadRequest("The review exceeds the maximum length of 260 characters.");
        }
        
        AuthResponse authResponse = await _userHelper.VerifyLogin(HttpContext.User.Identity);
        var entry = await _libraryRepository.GetEntryById(entryId);

        if (entry == null)
        {
            return NotFound();
        }
        
        var library = await _libraryRepository.GetLibraryByEntryId(entryId);

        if (!authResponse.IsAuthorized || authResponse.User.Id != library.UserId)
        {
            return Unauthorized();
        }
        
        try
        {
            await _libraryRepository.SetReview(entryId, review);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
        return Ok();
    }
}
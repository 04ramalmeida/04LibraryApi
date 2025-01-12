using _04LibraryApi.Data.Entities;
using _04LibraryApi.Data.Models;
using _04LibraryApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _04LibraryApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookController : ControllerBase
{
    
    private readonly IBookRepository _bookRepository;

    public BookController(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }
    
    [Authorize(Roles = "Librarian")]
    [HttpPost("[action]")]
    public IActionResult AddBook(BookInfo bookInfo)
    {
        Book newBook = new Book
        {
            Name = bookInfo.Name,
            Author = bookInfo.Author,
            Publisher = bookInfo.Publisher,
            Genre = bookInfo.Genre,
            PublishDate = DateTime.Parse(bookInfo.PublishDate),
            ImageURL = bookInfo.ImageURL
        };
        
        try
        {
            _bookRepository.CreateAsync(newBook);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        return Ok("Book added");
    }

    [Authorize]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetBook(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id);

        if (book == null)
        {
            return NotFound();
        }
        return Ok(book);
    }

    [Authorize(Roles = "Librarian")]
    [HttpPost("[action]")]
    public async Task<IActionResult> UpdateBook(int id, [FromBody]BookInfo bookInfo)
    {
        var book = await _bookRepository.GetByIdAsync(id);

        if (book == null)
        {
            return NotFound();
        }
        
        book.Name = bookInfo.Name;
        book.Author = bookInfo.Author;
        book.Publisher = bookInfo.Publisher;
        book.Genre = bookInfo.Genre;
        book.PublishDate = DateTime.Parse(bookInfo.PublishDate);
        book.ImageURL = bookInfo.ImageURL;

        try
        {
            await _bookRepository.UpdateAsync(book);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        
        return Ok("Book updated");
    }

    [Authorize(Roles = "Librarian")]
    [HttpPost("[action]")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id);

        if (book == null)
        {
            return NotFound();
        }

        try
        {
            await _bookRepository.DeleteAsync(book);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        
        return Ok("Book deleted");
    }

    
}
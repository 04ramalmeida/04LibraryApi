using System.ComponentModel.DataAnnotations;
namespace _04LibraryApi.Data.Entities;


public class ExpiredToken
{
    public int Id { get; set; }
    
    public string Token { get; set; }
}
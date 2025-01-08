using _04LibraryApi.Data;

namespace _04LibraryApi.Helpers;

public interface IMailHelper
{
    Response SendEmail(string to, string subject, string body);
}
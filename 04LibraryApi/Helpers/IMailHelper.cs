using _04LibraryApi.Data;

namespace _04LibraryApi.Helpers;

public interface IMailHelper
{
    MailResponse SendEmail(string to, string subject, string body);
}
namespace _04LibraryApi.Data;

public class MailResponse
{
    public bool IsSuccess { get; set; }

    public string Message { get; set; }

    public object Results { get; set; }
}
namespace _04LibraryApi.Data;

public class Response
{
    public bool IsSuccess { get; set; }

    public string Message { get; set; }

    public object Results { get; set; }
}
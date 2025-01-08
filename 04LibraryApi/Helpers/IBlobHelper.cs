using Azure.Storage.Blobs.Models;

namespace _04LibraryApi.Helpers;

public interface IBlobHelper
{
    Task<Guid> UploadBlobAsync(IFormFile file);

}
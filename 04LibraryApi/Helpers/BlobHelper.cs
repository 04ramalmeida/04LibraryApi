using _04LibraryApi.Data;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace _04LibraryApi.Helpers;

public class BlobHelper : IBlobHelper
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _blobContainerClient;

    public BlobHelper(IConfiguration configuration)
    {
        string connectionString = configuration["AzureString:Blob"];
        _blobServiceClient = new BlobServiceClient(connectionString);
        _blobContainerClient = _blobServiceClient.GetBlobContainerClient(configuration["AzureString:Blob"]);
    }

    public async Task<Guid> UploadBlobAsync(IFormFile file)
    {
            Guid guid = Guid.NewGuid();
            Stream stream = file.OpenReadStream();
            var blobClient = _blobContainerClient.GetBlobClient(guid.ToString());
            var result = await blobClient.UploadAsync(stream, true);
            return guid;
    }
}
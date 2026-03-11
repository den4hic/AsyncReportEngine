using AsyncReportEngine.Services.Abstraction;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text;

namespace AsyncReportEngine.Services;

public class BlobService : IBlobService
{
    private readonly BlobServiceClient blobServiceClient;
    private const string ContainerName = "generated-reports";

    public BlobService(BlobServiceClient blobServiceClient)
    {
        this.blobServiceClient = blobServiceClient;
    }

    public async Task<string> UploadReportAsync(string fileName, string csvContent)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobClient = containerClient.GetBlobClient(fileName);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        await blobClient.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = "text/csv" }
        });

        return blobClient.Uri.ToString();
    }
}

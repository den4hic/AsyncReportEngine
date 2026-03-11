namespace AsyncReportEngine.Services.Abstraction;

public interface IBlobService
{
    Task<string> UploadReportAsync(string fileName, string csvContent);
}

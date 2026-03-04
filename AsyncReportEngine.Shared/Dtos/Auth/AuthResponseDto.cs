namespace AsyncReportEngine.Shared.Dtos.Auth;

public class AuthResponseDto
{
    public bool IsSuccessful { get; set; }
    public string? Token { get; set; }
    public string? ErrorMessage { get; set; }
}

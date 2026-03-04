using AsyncReportEngine.Shared.Dtos.Auth;

namespace AsyncReportEngine.Services.Abstraction;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}

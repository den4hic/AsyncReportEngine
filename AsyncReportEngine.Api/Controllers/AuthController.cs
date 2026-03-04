using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos.Auth;
using Microsoft.AspNetCore.Mvc;

namespace AsyncReportEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;

    public AuthController(IAuthService authService)
    {
        this.authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await authService.RegisterAsync(dto);
        if (!result.IsSuccessful)
            return BadRequest(result.ErrorMessage);

        return Ok(new { Message = "Реєстрація успішна. Тепер ви можете увійти." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await authService.LoginAsync(dto);
        if (!result.IsSuccessful)
            return Unauthorized(result.ErrorMessage);

        return Ok(new { Token = result.Token });
    }
}

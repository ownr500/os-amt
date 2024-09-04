using API.Controllers.DTO;
using API.Extensions;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestDto requestDto)
    {
        var result = await _userService.RegisterAsync(requestDto.ToRequest());
        if (result.IsSuccess) return Ok();
        return new ConflictObjectResult(new BusinessErrorDto(
            result.GetErrors()
        ));
    }

    [HttpPost("signin")]
    public async Task<ActionResult<SinginResponseDto>> SigninAsync([FromBody] SigninRequestDto signinRequestDto, CancellationToken ct)
    {
        var result = await _userService.SingInAsync(signinRequestDto.ToModel(), ct);

        if (result.IsSuccess) return new SinginResponseDto(result.Value.AccessToken, result.Value.RefreshToken);

        return new ConflictObjectResult(new BusinessErrorDto(result.ToResult().GetErrors()));
    }
}
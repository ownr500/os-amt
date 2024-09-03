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
    public async Task<ActionResult<RegisterResponseDto>> Register(
        [FromBody] RegisterRequestDto requestDto)
    {
        var response = await _userService.RegisterAsync(requestDto.ToRequest());
        return response.ToDto();
    }

    [HttpPost("signin")]
    public async Task<ActionResult<SinginResponseDto>> SigninAsync([FromBody] SigninRequestDto signinRequestDto)
    {
        var result = await _userService.SinginAsync(signinRequestDto.ToModel());

        if (result.IsSuccess) return new SinginResponseDto(result.Value.AuthToken);

        return new ConflictObjectResult(new BusinessErrorDto(result.Errors.Select(x => x.Message).ToList()));
    }
}
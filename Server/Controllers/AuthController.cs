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
    private readonly ITokenService _tokenService;

    public AuthController(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }
    
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponseDto>> Register(
        [FromBody] RegisterRequestDto requestDto)
    {
        var response = await _userService.RegisterAsync(requestDto.ToRequest());
        return response.ToDto();
    }

    [HttpPost("signin")]
    public async Task<ActionResult<SinginResponseDto>> Signin([FromBody] SigninRequestDto signinRequestDto)
    {
        var user = await _userService.Singin(signinRequestDto.ToModel());

        if (user == null)
        {
            return new SinginResponseDto(null, "User not found.");
        }

        var token = _tokenService.GenerateAuthToken(user);

        await _tokenService.SaveToken(token);
        
        return new SinginResponseDto(
            token, null
        );
    }
}
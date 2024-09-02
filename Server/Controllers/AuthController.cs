using API.Controllers.DTO;
using API.Extensions;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

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
    public async Task<ActionResult<SinginResponseDto>> Signin([FromBody] SigninRequestDto signinRequestDto)
    {
        var user = await _userService.Singin(signinRequestDto.ToModel());


        // var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        // if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer"))
        // {
        //     return Unauthorized();
        // }
        //
        // var token = authHeader.Substring("Bearer".Length).Trim();
        //
        // try
        // {
        //     var claimsPrincipal = 
        // }
        // catch (SecurityTokenException  e)
        // {
        //     return Unauthorized("Invalid token");
        // }
    }
}
using API.Controllers.DTO;
using API.Extensions;
using API.Models.Response;
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
}
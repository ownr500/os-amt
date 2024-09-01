using API.Controllers.DTO;
using API.Extensions;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpDelete]
    public async Task<ActionResult<DeleteResponseDto>> Delete([FromQuery] string login)
    {
        var response = await _userService.DeleteAsync(login);
        return response.ToDto();
    }

    [HttpPatch]
    public async Task<ActionResult<ChangeResponseDto>> Change([FromBody] ChangeRequestDto requestDto)
    {
        var response = await _userService.ChangeAsync(requestDto.ToRequest());
        return response.ToDto();
    }
}
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

    [HttpDelete("{login}")]
    public async Task<IActionResult> Delete([FromRoute] string login)
    {
        var result = await _userService.DeleteAsync(login, HttpContext.RequestAborted);
        if (result.IsSuccess) return NoContent();
        return new ConflictObjectResult(new BusinessErrorDto(result.Errors.Select(x => x.Message).ToList()));
    }

    [HttpPatch]
    public async Task<ActionResult<ChangeResponseDto>> Change([FromBody] ChangeRequestDto requestDto)
    {
        var response = await _userService.ChangeAsync(requestDto.ToRequest());
        return response.ToDto();
    }
}
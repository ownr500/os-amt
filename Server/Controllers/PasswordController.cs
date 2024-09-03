using API.Controllers.DTO;
using API.Extensions;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PasswordController : ControllerBase
{
    private readonly IUserService _userService;

    public PasswordController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> Change([FromBody] PasswordChangeDto passwordChangeDto)
    {
        var result = await _userService.PasswordChangeAsync(passwordChangeDto.ToModel());
        if (result.IsSuccess) return new OkResult();
        return new ConflictObjectResult(new BusinessErrorDto(result.Errors.Select(x => x.Message).ToList()));
    }
}
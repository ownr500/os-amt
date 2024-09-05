using API.Controllers.DTO;
using API.Extensions;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuperAdminController : ControllerBase
{
    private readonly IUserService _userService;

    public SuperAdminController(IUserService userService)
    {
        _userService = userService;
    }
    [HttpPost("makeUserAdmin")]
    public async Task<IActionResult> MakeUserAdmin([FromForm] UserToAdminDto userToAdminDto)
    {
        var result = await _userService.UserToAdmin(userToAdminDto.ToModel());
        if (result.IsSuccess) return Ok();
        return new ConflictObjectResult(new BusinessErrorDto(result.GetErrors()));
    }
}
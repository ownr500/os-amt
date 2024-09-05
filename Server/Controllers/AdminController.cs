using API.Controllers.DTO;
using API.Extensions;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminController(IUserService userService)
    {
        _userService = userService;
    }
    [HttpGet("getUsers")]
    public async Task<ActionResult<UserListResponseDto>> GetUsers(CancellationToken ct)
    {
        var result = await _userService.GetUsers(ct);
        return new UserListResponseDto(
            result.ToDto()
        );
    }
}
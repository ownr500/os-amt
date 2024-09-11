using API.Controllers.DTO;
using API.Extensions;
using API.Models.enums;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize(Roles = nameof(RoleName.Admin))]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public AdminController(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }
    [HttpGet("getUsers")]
    public async Task<ActionResult<UserListResponseDto>> GetUsers(CancellationToken ct)
    {
        var result = await _userService.GetUsers(ct);
        return new UserListResponseDto(
            result.ToDtoList()
        );
    }

    [HttpPost("makeAdmin")]
    public async Task<IActionResult> MakeUserAdmin([FromQuery] Guid userId, CancellationToken ct)
    {
        var result = await _userService.MakeUserAdmin(userId, ct);
        if (result.IsSuccess) return Ok();
        return new ConflictObjectResult(new BusinessErrorDto(result.GetErrors()));
    }

    [HttpPost("revoke/{userId:guid}")]
    public async Task<IActionResult> Revoke([FromRoute] Guid userId, CancellationToken ct)
    {
        var result = await _tokenService.RevokeTokens(userId, ct);
        if (result.IsSuccess) return Ok();
        return new ConflictObjectResult(new BusinessErrorDto(result.GetErrors()));
    }
}
using API.Controllers.DTO;
using API.Extensions;
using API.Models.enums;
using API.Services.Interfaces;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize(Roles = nameof(RoleName.Admin))]
[Route("api/[controller]")]
public class AdminUsersController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;

    public AdminUsersController(ITokenService tokenService, IUserService userService)
    {
        _tokenService = tokenService;
        _userService = userService;
    }
    
    [HttpPost("{id:guid}/add-role")]
    public async Task<IActionResult> AddRole([FromRoute] Guid id, [FromBody] RoleName role, CancellationToken ct)
    {
        var result = await _userService.AddRoleAsync(id, role, ct);
        if (result.IsSuccess) return Ok();
        return new ConflictObjectResult(new BusinessErrorDto(result.GetErrors()));
    }

    [HttpPost("{id:guid}/remove-role")]
    public async Task<IActionResult> RemoveRole([FromRoute] Guid id, [FromBody] RoleName role, CancellationToken ct)
    {
        Result result = await _userService.RemoveRoleAsync(id, role, ct);
        if (result.IsSuccess) return Ok();
        return new ConflictObjectResult(new BusinessErrorDto(result.GetErrors()));
    }
    
    [HttpGet("get-users")]
    public async Task<ActionResult<UserListResponseDto>> GetUsers(CancellationToken ct)
    {
        var result = await _userService.GetUsers(ct);
        return new UserListResponseDto(
            result.ToDtoList()
        );
    }
    
    [HttpPost("{id:guid}/revoke-all-tokens")]
    public async Task<IActionResult> Revoke([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _tokenService.RevokeTokens(id, ct);
        if (result.IsSuccess) return Ok();
        return new ConflictObjectResult(new BusinessErrorDto(result.GetErrors()));
    }
}
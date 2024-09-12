using API.Controllers.DTO;
using API.Extensions;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TokensController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;

    public TokensController(ITokenService tokenService, IUserService userService)
    {
        _tokenService = tokenService;
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost("refresh/{token}")]
    public async Task<ActionResult<SinginResponseDto>> Refresh([FromRoute] string token, CancellationToken ct)
    {
        var result = await _tokenService.GenerateNewTokenFromRefreshTokenAsync(token,ct);
        if (result.IsSuccess) return new SinginResponseDto(result.Value.AccessToken, result.Value.RefreshToken);
        return new ConflictObjectResult(new BusinessErrorDto(result.GetErrors()));
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(CancellationToken ct)
    {
        var userId = _userService.GetUserIdFromContext();
        await _tokenService.RevokeTokens(userId, ct);
        return Ok();
    }
}
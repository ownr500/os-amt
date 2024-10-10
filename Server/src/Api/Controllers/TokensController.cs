using API.Controllers.Dtos;
using API.Core.Services;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TokensController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public TokensController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [AllowAnonymous]
    [HttpPost("refresh/{token}")]
    public async Task<ActionResult<RefreshResponseDto>> Refresh([FromRoute] string token, CancellationToken ct)
    {
        var result = await _tokenService.GenerateNewTokenFromRefreshTokenAsync(token,ct);
        if (result.IsSuccess) return new RefreshResponseDto(result.Value.AccessToken, result.Value.RefreshToken);
        return new ConflictObjectResult(new BusinessErrorDto(result.GetErrors()));
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(CancellationToken ct)
    {
        await _tokenService.RevokeTokensAsync(ct);
        return Ok();
    }
}
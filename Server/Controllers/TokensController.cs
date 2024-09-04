using API.Controllers.DTO;
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

    public TokensController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost("refresh/{token}")]
    public async Task<ActionResult<SinginResponseDto>> Refresh([FromRoute] string token)
    {
        var result = await _tokenService.GenerateNewTokenFromRefreshToken(token);
        if (result.IsSuccess) return new SinginResponseDto(result.Value.AccessToken, result.Value.RefreshToken);
        return new ConflictObjectResult(new BusinessErrorDto(result.Errors.Select(x => x.Message).ToList()));
    }
}
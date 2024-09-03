using System.Security.Claims;
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
    private readonly HttpContextAccessor _contextAccessor;
    private readonly ITokenService _tokenService;

    public PasswordController(IUserService userService, HttpContextAccessor contextAccessor, ITokenService tokenService)
    {
        _userService = userService;
        _contextAccessor = contextAccessor;
        _tokenService = tokenService;
    }

    [HttpPost]
    public async Task<IActionResult> Change([FromBody] PasswordChangeDto passwordChangeDto)
    {
        var authHeader = HttpContext.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer"))
        {
            return new UnauthorizedResult();
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

   
            var claimsPrincipal = await _tokenService.ValidateToken(token);
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                var result = await _userService.PasswordChangeAsync(passwordChangeDto.ToModel(new Guid(userId)));
                return result ? new OkResult() : new BadRequestResult();
            }
     
        return new BadRequestResult();
    }
}
using System.ComponentModel.DataAnnotations;
using API.Constants;
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
    public async Task<IActionResult> Change([FromBody] PasswordChangeDto passwordChangeDto, CancellationToken ct)
    {
        var result = await _userService.PasswordChangeAsync(passwordChangeDto.ToModel(), ct);
        if (result.IsSuccess) return Ok();
        return new ConflictObjectResult(new BusinessErrorDto(result.GetErrors()));
    }

    [AllowAnonymous]
    [HttpGet("recovery-link")]
    public async Task<IActionResult> SendRecoveryLink([FromBody] 
        [EmailAddress]
        [StringLength(ValidationConstants.MaxEmailLength, MinimumLength = ValidationConstants.MinEmailLength)]
        string email, CancellationToken ct)
    {
        var result = await _userService.SendRecoveryLinkAsync(email, ct);
        if (result.IsSuccess) return Ok();
        return new ConflictObjectResult(new BusinessErrorDto(result.GetErrors()));
    }
}
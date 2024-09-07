using API.Models.enums;
using API.Services.Interfaces;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize(Roles = nameof(RoleName.Admin))]
[Route("api/[controller]")]
public class UserAdmin : ControllerBase
{
    private readonly IUserService _userService;

    public UserAdmin(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpPost("{id}/add-role")]
    public async Task<IActionResult> AddRole([FromRoute] string id, [FromBody] string role)
    {
        Result result = await _userService.AddRoleAsync(id, role);
    }
}
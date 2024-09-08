﻿using API.Controllers.DTO;
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
    private readonly IUserService _userService;

    public AdminUsersController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpPost("{id:guid}/add-role")]
    public async Task<IActionResult> AddRole([FromRoute] Guid id, [FromBody] RoleName role, CancellationToken ct)
    {
        var result = await _userService.AddRoleAsync(id, role, ct);
        if (result.IsSuccess) return Ok();
        return new ConflictObjectResult(new BusinessErrorDto(result.GetErrors()));
    }
}
using API.Controllers.DTO;
using API.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace API.Services.Interfaces;

public interface IUserService
{
    Task<ActionResult<RegisterResponseDto>> RegisterAsync(RegisterRequest request);
}
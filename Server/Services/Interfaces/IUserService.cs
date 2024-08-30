using API.Controllers.DTO;
using API.Models.Request;
using API.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace API.Services.Interfaces;

public interface IUserService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
}
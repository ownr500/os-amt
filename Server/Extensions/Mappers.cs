using API.Controllers.DTO;
using API.Models.Request;

namespace API.Extensions;

public static class Mappers
{
    public static RegisterRequest ToRequest(this RegisterRequestDto dto)
    {
        return new RegisterRequest(
            dto.FirstName,
            dto.LastName,
            dto.Age,
            dto.Login,
            dto.Password
        );
    }
}
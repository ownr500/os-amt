using API.Controllers.DTO;
using API.Models.Request;
using API.Models.Response;

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

    public static RegisterResponseDto ToDto(this RegisterResponse response)
    {
        var result = response.Id is not null;
        return new RegisterResponseDto(
            result,
            result ? null : "Registration failed"
                );
    }
}
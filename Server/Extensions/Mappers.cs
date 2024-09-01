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
    public static DeleteResponseDto ToDto(this DeleteResponse response)
    {
        var result = response.Id is not null;
        return new DeleteResponseDto(
            result,
            result ? null : "Registration failed"
                );
    }

    public static ChangeRequest ToRequest(this ChangeRequestDto requestDto)
    {
        return new ChangeRequest(
            requestDto.FirstName,
            requestDto.Age,
            requestDto.Password
        );
    }

    public static ChangeResponseDto ToDto(this ChangeResponse response)
    {
        var result = response.Id is not null;
        return new ChangeResponseDto(
            result,
            result ? null : "User update failed"
        );
    }

    public static PasswordChangeModel ToModel(this PasswordChangeDto dto)
    {
        return new PasswordChangeModel(
            dto.Login,
            dto.OldPassword,
            dto.NewPassword
        );
    }
}
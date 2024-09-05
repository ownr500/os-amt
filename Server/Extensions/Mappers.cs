using API.Controllers.DTO;
using API.Models.Request;
using FluentResults;

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
    
    public static ChangeRequest ToRequest(this ChangeRequestDto requestDto)
    {
        return new ChangeRequest(
            requestDto.FirstName,
            requestDto.LastName
        );
    }

    public static PasswordChangeModel ToModel(this PasswordChangeDto dto)
    {
        return new PasswordChangeModel(
            Login: dto.Login,
            OldPassword: dto.OldPassword,
            NewPassword: dto.NewPassword
        );
    }

    public static SinginRequestModel ToModel(this SigninRequestDto dto)
    {
        return new SinginRequestModel(
            dto.Login,
            dto.Password);
    }

    public static List<string> GetErrors(this Result result)
    {
        return result.Errors.Select(x => x.Message).ToList();
    }
    public static List<string> GetErrors<T>(this Result<T> result)
    {
        return result.Errors.Select(x => x.Message).ToList();
    }
}
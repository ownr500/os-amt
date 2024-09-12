using API.Controllers.DTO;
using API.Models;
using API.Models.Entities;
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
            dto.Email,
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
            CurrentPassword: dto.CurrentPassword,
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

    public static UserModel ToModel(this UserEntity user)
    {
        return new UserModel(
           user.Id,
           user.FirstName,
           user.LastName,
           user.Login,
           user.UserRoles.Select(x => x.Role.RoleName).ToList()
            );
    }

    public static List<UserDto> ToDtoList(this List<UserModel> list)
    {
        var userDtos = list.Select(x => x.ToDto()).ToList();
        return userDtos;
    }

    public static UserDto ToDto(this UserModel model)
    {
        return new UserDto(
            model.UserId,
            model.FirstName,
            model.LastName,
            model.Login,
            model.Roles
        );
    }
}
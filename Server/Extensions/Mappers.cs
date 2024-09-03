﻿using API.Constants;
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
            result ? null : MessageConstants.RegistrationFailed
                );
    }
    public static DeleteResponseDto ToDto(this DeleteResponse response)
    {
        var result = response.Id is not null;
        return new DeleteResponseDto(
            result,
            result ? null : MessageConstants.RegistrationFailed
                );
    }

    public static ChangeRequest ToRequest(this ChangeRequestDto requestDto)
    {
        return new ChangeRequest(
            requestDto.FirstName,
            requestDto.LastName
        );
    }

    public static ChangeResponseDto ToDto(this ChangeResponse response)
    {
        var result = response.Id is not null;
        return new ChangeResponseDto(
            result,
            result ? null : MessageConstants.UserUpdateFailed
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
}
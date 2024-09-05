using FluentResults;

namespace API.Constants;

public class MessageConstants
{
    public const string UserNotFound = "User not found";
    public const string UserAlreadyRegistered = "User already registered";
    public const string RegistrationFailed = "Registration failed";
    public const string InvalidCredentials = "Invalid credentials";
    public const string OldPasswordNotMatch = "Old password doesn't match";
    public const string WrongPassword = "Wrong password";
    public const string UserUpdateFailed = "User update failed";
    public const string InvalidRefreshToken = "Invalid refresh token";
    public const string UserHasNoRoles = "User has no roles";
    public const string UserAlreadyAdmin = "User already admin";
    public const string NoActiveTokens = "User has no active authorizations";
}
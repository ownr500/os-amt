﻿using API.Models.enums;

namespace API.Constants;

public static class RoleConstants
{
    public static readonly Guid AdminRoleId = new ("C9A36382-BB77-4EE7-8539-681026B43916");
    public static readonly Guid UserRoleId = new ("561622CB-CA02-4C14-9C44-21BC4BA4D2AC");

    public static readonly Dictionary<RoleNames, Guid> RoleNameToGuid = new()
    {
        { RoleNames.Admin, AdminRoleId },
        { RoleNames.User, UserRoleId }
    };
}
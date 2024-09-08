using API.Models.enums;

namespace API.Constants;

public class RoleConstants
{
    public static readonly Guid AdminGuid = new Guid("C9A36382-BB77-4EE7-8539-681026B43916");
    public static readonly Guid UserGuid = new Guid("561622CB-CA02-4C14-9C44-21BC4BA4D2AC");

    public static readonly Dictionary<RoleName, Guid> RoleNameToGuid = new Dictionary<RoleName, Guid>
    {
        { RoleName.Admin, AdminGuid },
        { RoleName.User, UserGuid }
    };

    public static readonly Dictionary<Guid, RoleName> GuidToRoleName = new Dictionary<Guid, RoleName>
    {
        { AdminGuid, RoleName.Admin },
        { UserGuid, RoleName.User }
    };
}
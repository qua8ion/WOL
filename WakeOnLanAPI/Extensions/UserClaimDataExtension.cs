using Dto.Services.UserSecurityManager;
using Services.User.Provider;

namespace Extensions;

public static class UserClaimDataExtension
{
    public static bool VerifyAdminRights(this UserClaimData user, bool withException = false)
    {
        if (!user.Admin) return withException ? throw new UserProviderException.AccessDenied(UserProviderException.AccessDenied.NEED_ADMIN): false;
        else return true;
    }

    public static bool Verify(this UserClaimData? user, bool withException = false)
    {
        if (user is null) return withException ? throw new UserProviderException.AccessDenied(UserProviderException.AccessDenied.NEED_USER): false;
        else return true;
    }
}
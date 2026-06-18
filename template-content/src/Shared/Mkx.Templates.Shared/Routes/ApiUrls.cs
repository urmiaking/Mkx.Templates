using Mkx.Templates.Sdk.Shared.Extensions;

namespace Mkx.Templates.Shared.Routes;

public static class ApiUrls
{
    private static string BuildUrl(params string[] segments) => string.Join("/", segments);

    public static class Tests
    {
        public static string Get(Guid id) => BuildUrl(ApiRoutes.Tests.Base, ApiRoutes.Tests.Get)
            .FormatRoute(new { id });
    }

    public static class Accounts
    {
        public static string PerformExternalLogin() => BuildUrl(ApiRoutes.Accounts.Base, ApiRoutes.Accounts.PerformExternalLogin);
        public static string Logout(string? returnUrl) => 
            BuildUrl(ApiRoutes.Accounts.Base, ApiRoutes.Accounts.Logout);

        public static string Login(string? returnUrl) =>
            BuildUrl(ApiRoutes.Accounts.Base, ApiRoutes.Accounts.Login).AppendQueryString(new { returnUrl });
    }
}


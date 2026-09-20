namespace Mkx.Templates.Shared.Authorization;

public static class AppPolicies
{
    public static class Users
    {
        public const string View = $"{nameof(Mkx.Templates)}-{nameof(Users)}-{nameof(View)}";
        public const string Manage = $"{nameof(Mkx.Templates)}-{nameof(Users)}-{nameof(Manage)}";
        public const string ManageClaims = $"{nameof(Mkx.Templates)}-{nameof(Users)}-{nameof(ManageClaims)}";
    }

    public static class Tests
    {
        public const string View = $"{nameof(Mkx.Templates)}-{nameof(Tests)}-{nameof(View)}";
    }
}

namespace Mkx.Templates.Shared.Authorization;

public static class AppPolicies
{
    public static class Users
    {
        public const string View = $"{nameof(Mkx.Templates)}-{nameof(Users)}-{nameof(View)}";
        public const string Manage = $"{nameof(Mkx.Templates)}-{nameof(Users)}-{nameof(Manage)}";
        public const string ManageClaims = $"{nameof(Mkx.Templates)}-{nameof(Users)}-{nameof(ManageClaims)}";
    }

#if (IncludeSample)
    public static class SampleItems
    {
        public const string Manage = $"{nameof(Mkx.Templates)}-{nameof(SampleItems)}-{nameof(Manage)}";
        public const string View = $"{nameof(Mkx.Templates)}-{nameof(SampleItems)}-{nameof(View)}";
    }
#endif
}

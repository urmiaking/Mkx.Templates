namespace Mkx.Templates.Shared.Authorization;

public static class AppPolicies
{
    public static class Tests
    {
        public const string View = $"{nameof(Mkx.Templates)}-{nameof(Tests)}-{nameof(View)}";
    }
}
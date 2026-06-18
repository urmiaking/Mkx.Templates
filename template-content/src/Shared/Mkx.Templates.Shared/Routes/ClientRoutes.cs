namespace Mkx.Templates.Shared.Routes;

public static class ClientRoutes
{
    public static class General
    {
        public const string NotFound = "/not-found";
    }

    public static class Home
    {
        public const string Index = "/";
    }

    public static class Accounts
    {
        public const string Prefix = "/Account";
        public const string AccessDenied = $"{Prefix}/AccessDenied";
        public const string ExternalLogin = $"{Prefix}/ExternalLogin";
        public const string Login = $"{Prefix}/Login";
        public const string Lockout = $"{Prefix}/Lockout";
    }

    public static class Logs
    {
        public const string Base = "/serilog-ui";
    }

    public static class Tests
    {
        public const string Index = "/tests";
    }
}


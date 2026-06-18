namespace Mkx.Templates.Shared.Routes;

public static class ApiRoutes
{
    public class Tests
    {
        public const string Base = "/api/tests";
        public const string Create = "";
        public const string Get = "{id}";
    }

    public class Accounts
    {
        public const string Base = "/api/Account";

        public const string Login = "Login";
        public const string Logout = "Logout";
        public const string PerformExternalLogin = "PerformExternalLogin";

        public static class Manage
        {
            public const string Base = "Manage";
            public const string LinkExternalLogin = "LinkExternalLogin";
        }
    }

    public class Health
    {
        public const string Base = "/health";
    }
}


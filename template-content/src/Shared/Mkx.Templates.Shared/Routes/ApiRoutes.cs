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

    public class UserAccounts
    {
        public const string Base = "/api/user-accounts";
        public const string GetCurrentUser = "current-user";
        public const string UpdateFullName = "full-name";
        public const string UpdatePhoneNumber = "phone-number";
        public const string SendVerificationToken = "send-verification-token";
        public const string UpdateEmail = "email";
        public const string UpdatePassword = "password";
        public const string Get2FaStatus = "2fa";
        public const string ForgetDevice = "2fa/forget-device";
        public const string Disable2Fa = "2fa/disable";
        public const string GetAuthenticatorKey = "authenticator-key";
        public const string Enable2Fa = "2fa/enable";
        public const string GetExternalProviders = "external-providers";
        public const string ExternalLogin = "external-login";
        public const string ExternalLoginCallback = "external-login-callback";
        public const string GetPasskeys = "passkeys";
        public const string GetPasskeyCreationOptions = "passkeys/creation-options";
        public const string GetPasskeyRequestOptions = "passkeys/request-options";
        public const string AddPasskey = "passkeys/add";
        public const string RemovePasskey = "passkeys/{credentialId}/remove";
        public const string GetAccountsList = "list";
        public const string LockUser = "{id}/lock-user";
        public const string UnlockUser = "{id}/unlock-user";
        public const string CreateAccount = "user";
        public const string UpdateAccount = "user/{id}";
        public const string DeleteAccount = "user/{id}";
    }

    public class Health
    {
        public const string Base = "/health";
    }
}


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

    public static class UserAccounts
    {
        private const string UserAccountsPrefix = "user-accounts";

        public const string Index = $"{UserAccountsPrefix}";
        public const string ChangePassword = $"{UserAccountsPrefix}/change-password";
        public const string Disable2Fa = $"{UserAccountsPrefix}/disable-2fa";
        public const string Email = $"{UserAccountsPrefix}/email";
        public const string ExternalLogins = $"{UserAccountsPrefix}/external-logins";
        public const string Passkeys = $"{UserAccountsPrefix}/passkeys";
        public const string EnableAuthenticator = $"{UserAccountsPrefix}/enable-authenticator";
        public const string GenerateRecoveryCodes = $"{UserAccountsPrefix}/generate-recovery-codes";
        public const string ResetAuthenticator = $"{UserAccountsPrefix}/reset-authenticator";
        public const string SetPassword = $"{UserAccountsPrefix}/set-password";
        public const string TwoFactorAuthentication = $"{UserAccountsPrefix}/two-factor-authentication";
        public const string AccountsList = $"{UserAccountsPrefix}/list";
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


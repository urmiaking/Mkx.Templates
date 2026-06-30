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
            BuildUrl(ApiRoutes.Accounts.Base, ApiRoutes.Accounts.Logout).AppendQueryString(new { returnUrl });

        public static string Login(string? returnUrl) =>
            BuildUrl(ApiRoutes.Accounts.Base, ApiRoutes.Accounts.Login).AppendQueryString(new { returnUrl });
    }

    public static class UserAccounts
    {
        public static string GetCurrentUser() =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.GetCurrentUser);

        public static string UpdateFullName() =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.UpdateFullName);

        public static string UpdatePhoneNumber() =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.UpdatePhoneNumber);

        public static string SendVerificationToken() =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.SendVerificationToken);

        public static string UpdateEmail() =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.UpdateEmail);

        public static string UpdatePassword() =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.UpdatePassword);

        public static string Get2FaStatus() =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.Get2FaStatus);

        public static string ForgetDevice() =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.ForgetDevice);

        public static string Disable2Fa() =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.Disable2Fa);

        public static string GetAuthenticatorKey() =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.GetAuthenticatorKey);

        public static string Enable2Fa() =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.Enable2Fa);

        public static string GetExternalProviders() =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.GetExternalProviders);

        public static string ExternalLogin(string provider) =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.ExternalLogin).AppendQueryString(new { provider });

        public static string GetPasskeys() =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.GetPasskeys);

        public static string GetPasskeyCreationOptions() =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.GetPasskeyCreationOptions);

        public static string GetPasskeyRequestOptions(string? username) =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.GetPasskeyRequestOptions).AppendQueryString(new { username });

        public static string AddPasskey() =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.AddPasskey);

        public static string RemovePasskey(string credentialId) =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.RemovePasskey)
                .FormatRoute(new { credentialId });

        public static string GetAccountsList() =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.GetAccountsList);

        public static string LockUser(Guid id) =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.LockUser)
                .FormatRoute(new { id });

        public static string UnlockUser(Guid id) =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.UnlockUser)
                .FormatRoute(new { id });

        public static string CreateAccount() =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.CreateAccount);

        public static string UpdateAccount(Guid id) =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.UpdateAccount)
                .FormatRoute(new { id });

        public static string DeleteAccount(Guid id) =>
            BuildUrl(ApiRoutes.UserAccounts.Base, ApiRoutes.UserAccounts.DeleteAccount)
                .FormatRoute(new { id });
    }
}


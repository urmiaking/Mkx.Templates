using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.DTOs.Claims;
using Mkx.Templates.Shared.DTOs.Roles;
using Mkx.Templates.Shared.DTOs.Users;
using Mkx.Templates.Shared.Routes;
using Mkx.Templates.Sdk.Server.Shared.Exceptions;
using Mkx.Templates.Sdk.Shared.Attributes;
using Mkx.Templates.Sdk.Shared.Exceptions;
using System.Net.Http.Json;
using System.Text.Json;

namespace Mkx.Templates.Client.Services;

[ScopedService]
public class UserManagementClientService(HttpClient client, JsonSerializerOptions jsonOptions) : IUserManagementService
{
    public async Task<bool> IsUserManagementEnabledAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.UserManagement.Enabled(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<bool>(jsonOptions, cancellationToken);
        return result;
    }

    public async Task<List<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.UserManagement.GetUsers(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<UserDto>>(jsonOptions, cancellationToken);
        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.UserManagement.GetUserById(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        return await response.Content.ReadFromJsonAsync<UserDto>(jsonOptions, cancellationToken);
    }

    public async Task<bool> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.UserManagement.CreateUser(), dto, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        return true;
    }

    public async Task<bool> UpdateUserAsync(UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.UserManagement.UpdateUser(), dto, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync(ApiUrls.UserManagement.DeleteUser(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        return true;
    }

    public async Task<List<PolicyTreeNodeDto>> GetUserClaimsTreeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.UserManagement.GetUserClaimsTree(userId), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<PolicyTreeNodeDto>>(jsonOptions, cancellationToken);
        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<bool> UpdateUserClaimsAsync(Guid userId, List<string> grantedClaimNames, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.UserManagement.UpdateUserClaims(userId), grantedClaimNames, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        return true;
    }

    public async Task<List<RoleClaimsDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.UserManagement.GetRoles(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<RoleClaimsDto>>(jsonOptions, cancellationToken);
        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<List<PolicyTreeNodeDto>> GetRoleClaimsTreeAsync(string roleName, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.UserManagement.GetRoleClaimsTree(roleName), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<PolicyTreeNodeDto>>(jsonOptions, cancellationToken);
        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<bool> UpdateRoleClaimsAsync(string roleName, List<string> grantedClaimNames, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.UserManagement.UpdateRoleClaims(roleName), grantedClaimNames, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        return true;
    }
}

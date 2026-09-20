using Mkx.Templates.Shared.DTOs.Claims;
using Mkx.Templates.Shared.DTOs.Roles;
using Mkx.Templates.Shared.DTOs.Users;

namespace Mkx.Templates.Shared.Abstractions;

public interface IUserManagementService
{
    Task<List<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserAsync(UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<List<PolicyTreeNodeDto>> GetUserClaimsTreeAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserClaimsAsync(Guid userId, List<string> grantedClaimNames, CancellationToken cancellationToken = default);
    
    Task<List<RoleClaimsDto>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<List<PolicyTreeNodeDto>> GetRoleClaimsTreeAsync(string roleName, CancellationToken cancellationToken = default);
    Task<bool> UpdateRoleClaimsAsync(string roleName, List<string> grantedClaimNames, CancellationToken cancellationToken = default);

    Task<bool> IsUserManagementEnabledAsync(CancellationToken cancellationToken = default);
}

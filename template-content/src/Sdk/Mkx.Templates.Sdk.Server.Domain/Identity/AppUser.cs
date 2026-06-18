using Microsoft.AspNetCore.Identity;

namespace Mkx.Templates.Sdk.Server.Domain.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string Name { get; private set; } = default!;

    public ICollection<AppUserClaim> Claims { get; set; } = default!;
    public ICollection<AppUserLogin> Logins { get; set; } = default!;
    public ICollection<AppUserToken> Tokens { get; set; } = default!;
    public ICollection<AppUserRole> UserRoles { get; set; } = default!;
    public virtual ICollection<AppUserPasskey> Passkeys { get; set; } = default!;

    public AppUser(string name, string username, string? email = null, string? phoneNumber = null)
    {
        Id = Guid.CreateVersion7();
        Name = name;
        UserName = username;
        Email = email;
        PhoneNumber = phoneNumber;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private AppUser()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }

    public void SetName(string name) => Name = name;
}


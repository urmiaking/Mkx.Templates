using Microsoft.AspNetCore.Identity;

namespace Mkx.Templates.Sdk.Server.Domain.Identity;

public class AppRole : IdentityRole<Guid>
{
    public ICollection<AppUserRole>? UserRoles { get; set; }
    public ICollection<AppRoleClaim>? Claims { get; set; }

    public AppRole(string name)
    {
        Id = Guid.CreateVersion7();
        Name = name;
    }

    private AppRole() { }
}


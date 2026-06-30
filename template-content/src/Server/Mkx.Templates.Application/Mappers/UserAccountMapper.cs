using System;
using System.Linq;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Mkx.Templates.Sdk.Server.Domain.Identity;
using Mkx.Templates.Shared.DTOs.UserAccounts;
using Mkx.Templates.Shared.Utilities;

namespace Mkx.Templates.Application.Mappers;

internal sealed class UserAccountMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AppUser, GetUserAccountResponse>()
            .Map(dest => dest.Username, src => src.UserName)
            .Map(dest => dest.FullName, src => src.Name)
            .Map(dest => dest.Role, src => src.UserRoles != null && src.UserRoles.Any() ? src.UserRoles.First().Role.Name : string.Empty)
            .Map(dest => dest.IsActive, src => !src.LockoutEnd.HasValue || src.LockoutEnd.Value < DateTimeOffset.Now);

        config.NewConfig<UserPasskeyInfo, GetPasskeyResponse>()
            .Map(dest => dest.CredentialId, 
                src => Base64Url.Encode(src.CredentialId));
    }
}

using Mkx.Templates.Domain.TestAggregate;
using Mkx.Templates.Shared.DTOs.Tests;
using Mapster;

namespace Mkx.Templates.Application.Mappers;

internal sealed class TestMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Test, GetTestResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description);
    }
}

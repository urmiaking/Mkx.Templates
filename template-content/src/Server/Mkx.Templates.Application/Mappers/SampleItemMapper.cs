using Mkx.Templates.Domain.SampleItemAggregate;
using Mkx.Templates.Shared.DTOs.SampleItems;
using Mapster;

namespace Mkx.Templates.Application.Mappers;

internal sealed class SampleItemMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SampleItem, GetSampleItemResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Version, src => src.Version);
    }
}

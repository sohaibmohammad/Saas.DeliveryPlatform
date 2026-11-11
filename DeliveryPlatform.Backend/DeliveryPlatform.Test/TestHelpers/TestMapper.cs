using AutoMapper;
using DeliveryPlatform.Application.src.Common.Mappings;
using Microsoft.Extensions.Logging.Abstractions;

namespace DeliveryPlatform.Test.TestHelpers;

public static class TestMapper
{
    public static IMapper Create()
    {
        var configuration = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingProfile>(),
            NullLoggerFactory.Instance);

        return configuration.CreateMapper();
    }
}

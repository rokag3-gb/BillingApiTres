using AutoMapper;
using BillingApiTres.Models.MapperProfiles;

namespace BillingApi.Test.Fixtures
{
    public class AutoMapperFixture
    {
        public IMapper Mapper { get; }

        public AutoMapperFixture()
        {
            var profile = new ServiceHierarchyProfile();
            var configuration = new MapperConfiguration(config =>
            {
                config.AddProfile(profile);
                config.AddMaps("BillingApiTres");
            });
            Mapper = new Mapper(configuration);
        }
    }
}

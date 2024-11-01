using AutoMapper;
using Billing.EF.Repositories;
using BillingApi.Integration.Test.Fixtures;
using BillingApi.Test.Fixtures;
using BillingApiTres.Controllers.ServiceHierachies;
using BillingApiTres.Models.Clients;
using BillingApiTres.Models.MapperProfiles;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Http.Json;

namespace BillingApi.Integration.Test
{
    /// <summary>
    ///  데이터를 변경하지 않는 동작에 대한 fixture 사용 예제를 만들기 위해 작성된 클래스 입니다
    /// </summary>
    [Collection(nameof(TransactionalIamDatabaseFixture))]
    public class AccountServiceHeirarchyTest : IClassFixture<AutoMapperFixture>
    {
        private const string _bearerToken = "Bearer eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJ2S1Rwc3pCMjdfMzROejBpaW9BQm5ZT25SMndsbW1sVjRDeXVfOWtEdy1nIn0.eyJleHAiOjE3MzAyMTc2NDIsImlhdCI6MTczMDIxNTg0MiwianRpIjoiNzNjMGJmNTgtZThlMy00OWZhLTkyNjctMmU4MDI3MzQxZGNhIiwiaXNzIjoiaHR0cHM6Ly9kZXYua2V5LmFobmxhYmNsb3VkbWF0ZS5jb20vcmVhbG1zL0lBTSIsImF1ZCI6ImFjY291bnQiLCJzdWIiOiJkNmZiOGM2My04YzMwLTQwOTEtODRkNC1iYjJiMDgxZDI3YzMiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJpYW0iLCJzZXNzaW9uX3N0YXRlIjoiOWQ2YTVkZWEtNDM4Ny00NjZhLWI0MWUtZjgxNTJhZmVjYmQ0IiwiYWNyIjoiMSIsImFsbG93ZWQtb3JpZ2lucyI6WyIqIl0sInJlYWxtX2FjY2VzcyI6eyJyb2xlcyI6WyJkZWZhdWx0LXJvbGVzLWlhbSIsIm9mZmxpbmVfYWNjZXNzIiwidW1hX2F1dGhvcml6YXRpb24iXX0sInJlc291cmNlX2FjY2VzcyI6eyJhY2NvdW50Ijp7InJvbGVzIjpbIm1hbmFnZS1hY2NvdW50IiwibWFuYWdlLWFjY291bnQtbGlua3MiLCJ2aWV3LXByb2ZpbGUiXX19LCJzY29wZSI6ImVtYWlsIHByb2ZpbGUiLCJzaWQiOiI5ZDZhNWRlYS00Mzg3LTQ2NmEtYjQxZS1mODE1MmFmZWNiZDQiLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwibmFtZSI6IuOFjuOFheOFjiIsInByZWZlcnJlZF91c2VybmFtZSI6InNhbmdodW4uaGFuQGFobmxhYmNsb3VkbWF0ZS5jb20iLCJnaXZlbl9uYW1lIjoi44WO44WF44WOIiwiZmFtaWx5X25hbWUiOiIiLCJlbWFpbCI6InNhbmdodW4uaGFuQGFobmxhYmNsb3VkbWF0ZS5jb20ifQ.OHoALTvEU_UQaVjqhN58Iy3KfGVCtxjj1Syek-RtS65NuGol9T8w4Umi4fKzhMIc_wSU6dGPZZ0t7n4mQX3eImVt7Ls9b70YG2juVb0GeWO-X8Eb-Of9dWRuOHhPrPbBL3NYi6tkiXk_jiitw-VcMz1GAdLpvG52jFJ92km76iluf0wmJtPqs5nWPyGSR6YVw1jMzM0x1DNEUgRZe5ZEBz9rec-YTz7FtpI4zv-NCJfvLUNOGLXOe_iR49pRKJHeCXoEgnfmRGTYdQU2d6UEwnNLFgZ2aTQU9vcB97ADeVW_eRitoWqLlmlSb3tg6LYA1VJuoe24d6Vl8wXqrkLLCQ";
        private const string _baseAddress = "https://localhost";

        public TransactionalIamDatabaseFixture Fixture { get; }
        public AutoMapperFixture MapperFixture { get; }

        public AccountServiceHeirarchyTest(TransactionalIamDatabaseFixture fixture, AutoMapperFixture mapperFixture)
        {
            Fixture = fixture;
            MapperFixture = mapperFixture;
        }

        /// <summary>
        /// fixture 사용 테스트 용입니다. 통합 테스트 로직 작성 시 수정이 필요합니다.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task When_GetRepositoryTest_Expect_AnyData()
        {
            using var context = Fixture.CreateContext();

            var repository = new ServiceHierarchyRepository(context);

            var fakeLogger = new FakeLogger<GetServiceHierachyController>();
            var salesFakeLogger = new FakeLogger<SalesClient>();

            //통합 테스트이므로 httpclient도 테스트 대역이 아닌 실제 httpclient를 써야 하나?
            MockHttpMessageHandler handler = new();
            handler.Expect(HttpMethod.Get, $"{_baseAddress}/sales/account/1")
                   .Respond(HttpStatusCode.OK, JsonContent.Create(""));
            var httpClient = new HttpClient(handler);
            httpClient.BaseAddress = new Uri(_baseAddress);
            var salesClient = Substitute.For<SalesClient>(httpClient, salesFakeLogger);

            var sut = new GetServiceHierachyController(repository, MapperFixture.Mapper, salesClient, fakeLogger);
            //인증 서비스도 통합 테스트에 포함될 수 있다. fixture 동작 확인을 위한 예제용 코드라서 패스
            sut.ControllerContext.HttpContext = new DefaultHttpContext();
            sut.ControllerContext.HttpContext.Request.Headers["Authorization"] = _bearerToken;

            //act
            var result = await sut.Get(1);

            //assert
            Assert.NotNull(result.Value);
        }
    }
}

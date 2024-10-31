using AutoMapper;
using Billing.Data.Interfaces;
using Billing.Data.Models;
using BillingApiTres.Controllers.ServiceHierachies;
using BillingApiTres.Models.Clients;
using BillingApiTres.Models.MapperProfiles;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using System.Net;
using Microsoft.AspNetCore.Http;
using RichardSzalay.MockHttp;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;


namespace BillingApi.Test
{
    public class ServiceHierachyTest
    {
        private const int _topRootAccountId = 0;
        private const int _acmeAccountId = 1;
        private const int _parterAccountId1 = 2;
        private const int _parterAccountId2 = 3;
        private const int _partner1CustomerAccountId1 = 4;
        private const int _partner1CustomerAccountId2 = 5;
        private const int _partner2CustomerAccountId1 = 6;
        private const int _notExistAccountId = 84;
        private const string _bearerToken = "Bearer eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJ2S1Rwc3pCMjdfMzROejBpaW9BQm5ZT25SMndsbW1sVjRDeXVfOWtEdy1nIn0.eyJleHAiOjE3MzAyMTc2NDIsImlhdCI6MTczMDIxNTg0MiwianRpIjoiNzNjMGJmNTgtZThlMy00OWZhLTkyNjctMmU4MDI3MzQxZGNhIiwiaXNzIjoiaHR0cHM6Ly9kZXYua2V5LmFobmxhYmNsb3VkbWF0ZS5jb20vcmVhbG1zL0lBTSIsImF1ZCI6ImFjY291bnQiLCJzdWIiOiJkNmZiOGM2My04YzMwLTQwOTEtODRkNC1iYjJiMDgxZDI3YzMiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJpYW0iLCJzZXNzaW9uX3N0YXRlIjoiOWQ2YTVkZWEtNDM4Ny00NjZhLWI0MWUtZjgxNTJhZmVjYmQ0IiwiYWNyIjoiMSIsImFsbG93ZWQtb3JpZ2lucyI6WyIqIl0sInJlYWxtX2FjY2VzcyI6eyJyb2xlcyI6WyJkZWZhdWx0LXJvbGVzLWlhbSIsIm9mZmxpbmVfYWNjZXNzIiwidW1hX2F1dGhvcml6YXRpb24iXX0sInJlc291cmNlX2FjY2VzcyI6eyJhY2NvdW50Ijp7InJvbGVzIjpbIm1hbmFnZS1hY2NvdW50IiwibWFuYWdlLWFjY291bnQtbGlua3MiLCJ2aWV3LXByb2ZpbGUiXX19LCJzY29wZSI6ImVtYWlsIHByb2ZpbGUiLCJzaWQiOiI5ZDZhNWRlYS00Mzg3LTQ2NmEtYjQxZS1mODE1MmFmZWNiZDQiLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwibmFtZSI6IuOFjuOFheOFjiIsInByZWZlcnJlZF91c2VybmFtZSI6InNhbmdodW4uaGFuQGFobmxhYmNsb3VkbWF0ZS5jb20iLCJnaXZlbl9uYW1lIjoi44WO44WF44WOIiwiZmFtaWx5X25hbWUiOiIiLCJlbWFpbCI6InNhbmdodW4uaGFuQGFobmxhYmNsb3VkbWF0ZS5jb20ifQ.OHoALTvEU_UQaVjqhN58Iy3KfGVCtxjj1Syek-RtS65NuGol9T8w4Umi4fKzhMIc_wSU6dGPZZ0t7n4mQX3eImVt7Ls9b70YG2juVb0GeWO-X8Eb-Of9dWRuOHhPrPbBL3NYi6tkiXk_jiitw-VcMz1GAdLpvG52jFJ92km76iluf0wmJtPqs5nWPyGSR6YVw1jMzM0x1DNEUgRZe5ZEBz9rec-YTz7FtpI4zv-NCJfvLUNOGLXOe_iR49pRKJHeCXoEgnfmRGTYdQU2d6UEwnNLFgZ2aTQU9vcB97ADeVW_eRitoWqLlmlSb3tg6LYA1VJuoe24d6Vl8wXqrkLLCQ";
        private const string _baseAddress = "https://localhost";

        /// <summary>
        /// acme의 서비스 계층 구조를 조회할 경우 파트너와 고객을 모두 포함한 데이터를 반환하는지 확인합니다.
        /// </summary>
        [Fact]
        public async Task When_Acme_Expect_HasPartersAndCustomers()
        {
            //arrange
            var serviceHierarchyRepositoryStub = Substitute.For<IServiceHierarchyRepository>();
            serviceHierarchyRepositoryStub
                .GetParent(_acmeAccountId)
                .Returns(x => Task.FromResult(new ServiceHierarchy
                {
                    ParentAccId = _topRootAccountId,
                    AccountId = _acmeAccountId
                }));
            serviceHierarchyRepositoryStub
                .GetChild(_acmeAccountId)
                .Returns(x => Task.FromResult(CreateAcmeChilds()));
            serviceHierarchyRepositoryStub
                .GetChild(_parterAccountId1)
                .Returns(x => Task.FromResult(CreatePartner1Child()));
            serviceHierarchyRepositoryStub
                .GetChild(_parterAccountId2)
                .Returns(x => Task.FromResult(CreatePartner2Child()));

            var mapper = CreateMapper();
            var fakeLogger = new FakeLogger<GetServiceHierachyController>();
            var salesFakeLogger = new FakeLogger<SalesClient>();

            MockHttpMessageHandler handler = new();
            handler.Expect(HttpMethod.Get, $"{_baseAddress}/sales/account?limit=99999")
                   .Respond(HttpStatusCode.OK, JsonContent.Create(""));
            var httpClient = CreateHttpClientStub(handler);
            var salesClient = Substitute.For<SalesClient>(httpClient, salesFakeLogger);

            var sut = new GetServiceHierachyController(serviceHierarchyRepositoryStub, mapper, salesClient, fakeLogger);
            sut.ControllerContext.HttpContext = new DefaultHttpContext();
            sut.ControllerContext.HttpContext.Request.Headers["Authorization"] = _bearerToken;

            //act
            var result = await sut.GetList(_acmeAccountId);

            //assert
            Assert.NotNull(result?.Value);
            Assert.True(result.Value.Count == 6);
            Assert.Contains(result.Value, s => s.ContractorId == _topRootAccountId && s.ContracteeId == _acmeAccountId);
            Assert.Contains(result.Value, s => s.ContractorId == _acmeAccountId && s.ContracteeId == _parterAccountId1);
            Assert.Contains(result.Value, s => s.ContractorId == _acmeAccountId && s.ContracteeId == _parterAccountId2);
            Assert.Contains(result.Value, s => s.ContractorId == _parterAccountId1 && s.ContracteeId == _partner1CustomerAccountId1);
            Assert.Contains(result.Value, s => s.ContractorId == _parterAccountId1 && s.ContracteeId == _partner1CustomerAccountId2);
            Assert.Contains(result.Value, s => s.ContractorId == _parterAccountId2 && s.ContracteeId == _partner2CustomerAccountId1);
        }

        /// <summary>
        /// 파트너의 서비스 계층 구조를 조회할 경우 대상 파트너를 기준으로 데이터를 반환하는지 확인합니다.
        /// </summary>
        [Fact]
        public async Task When_Partner_Expect_HasCumstomers()
        {
            //arrange
            var serviceHierarchyRepositoryStub = Substitute.For<IServiceHierarchyRepository>();
            serviceHierarchyRepositoryStub
                .GetParent(_parterAccountId1)
                .Returns(x => Task.FromResult(new ServiceHierarchy { AccountId = _parterAccountId1, ParentAccId = _acmeAccountId }));
            serviceHierarchyRepositoryStub
                .GetChild(_parterAccountId1)
                .Returns(x => Task.FromResult(CreatePartner1Child()));

            var mapper = CreateMapper();
            var fakeLogger = new FakeLogger<GetServiceHierachyController>();
            var salesFakeLogger = new FakeLogger<SalesClient>();

            MockHttpMessageHandler handler = new();
            handler.Expect(HttpMethod.Get, $"{_baseAddress}/sales/account?limit=99999")
                   .Respond(HttpStatusCode.OK, JsonContent.Create(""));
            var httpClient = CreateHttpClientStub(handler);
            var salesClient = Substitute.For<SalesClient>(httpClient, salesFakeLogger);

            var sut = new GetServiceHierachyController(serviceHierarchyRepositoryStub, mapper, salesClient, fakeLogger);
            sut.ControllerContext.HttpContext = new DefaultHttpContext();
            sut.ControllerContext.HttpContext.Request.Headers["Authorization"] = _bearerToken;

            //act
            var partnerAccount1Result = await sut.GetList(_parterAccountId1);

            //assert
            Assert.NotNull(partnerAccount1Result.Value);
            Assert.True(partnerAccount1Result.Value.Count == 4);
            Assert.Contains(partnerAccount1Result.Value, s => s.ContractorId == _acmeAccountId && s.ContracteeId == _parterAccountId1);
            Assert.Contains(partnerAccount1Result.Value, s => s.ContractorId == _parterAccountId1 && s.ContracteeId == _partner1CustomerAccountId1);
            Assert.Contains(partnerAccount1Result.Value, s => s.ContractorId == _parterAccountId1 && s.ContracteeId == _partner1CustomerAccountId2);
        }

        /// <summary>
        /// 고객의 서비스 계층 구조를 조회할 경우 대상 고객을 기준으로 데이터를 반환하는지 확인합니다.
        /// </summary>
        [Fact]
        public async Task When_Customer_Expect_HasPartner()
        {
            //arrange
            var serviceHierarchyRepositoryStub = Substitute.For<IServiceHierarchyRepository>();
            serviceHierarchyRepositoryStub
                .GetParent(_partner1CustomerAccountId1)
                .Returns(x => Task.FromResult(new ServiceHierarchy { AccountId = _partner1CustomerAccountId1, ParentAccId = _parterAccountId1 }));
            serviceHierarchyRepositoryStub
                .GetChild(_parterAccountId1)
                .Returns(x => Task.FromResult(CreatePartner1Child()));

            var mapper = CreateMapper();
            var fakeLogger = new FakeLogger<GetServiceHierachyController>();
            var salesFakeLogger = new FakeLogger<SalesClient>();

            MockHttpMessageHandler handler = new();
            handler.Expect(HttpMethod.Get, $"{_baseAddress}/sales/account?limit=99999")
                   .Respond(HttpStatusCode.OK, JsonContent.Create(""));
            var httpClient = CreateHttpClientStub(handler);
            var salesClient = Substitute.For<SalesClient>(httpClient, salesFakeLogger);

            var sut = new GetServiceHierachyController(serviceHierarchyRepositoryStub, mapper, salesClient, fakeLogger);
            sut.ControllerContext.HttpContext = new DefaultHttpContext();
            sut.ControllerContext.HttpContext.Request.Headers["Authorization"] = _bearerToken;

            //act
            var partner1CustomerAccount1Result = await sut.GetList(_partner1CustomerAccountId1);

            //assert
            Assert.NotNull(partner1CustomerAccount1Result.Value);
            Assert.True(partner1CustomerAccount1Result.Value.Count == 2);
            Assert.Contains(partner1CustomerAccount1Result.Value, s => s.ContractorId == _parterAccountId1 && s.ContracteeId == _partner1CustomerAccountId1);
        }

        /// <summary>
        /// 서비스 계층 구조가 존재하지 않는 account id를 조회할 경우 NoContent 상태를 반환하는지 확인합니다
        /// </summary>
        [Fact]
        public async Task When_NotExistAccountId_Expect_NotContent()
        {
            //arrange
            var serviceHierarchyRepositoryStub = Substitute.For<IServiceHierarchyRepository>();

            var mapper = CreateMapper();
            var fakeLogger = new FakeLogger<GetServiceHierachyController>();
            var salesFakeLogger = new FakeLogger<SalesClient>();

            MockHttpMessageHandler handler = new();
            handler.Expect(HttpMethod.Get, $"{_baseAddress}/sales/account?limit=99999")
                   .Respond(HttpStatusCode.OK, JsonContent.Create(""));
            var httpClient = CreateHttpClientStub(handler);
            var salesClient = Substitute.For<SalesClient>(httpClient, salesFakeLogger);

            var sut = new GetServiceHierachyController(serviceHierarchyRepositoryStub, mapper, salesClient, fakeLogger);

            //act
            var result = await sut.GetList(_notExistAccountId);

            //assert
            Assert.IsAssignableFrom<NoContentResult>(result.Result);
        }
        

        private List<ServiceHierarchy> CreateAcmeChilds()
        {
            return new List<ServiceHierarchy>
            {
                new ServiceHierarchy
                {
                    AccountId = _parterAccountId1,
                    ParentAccId = _acmeAccountId
                },
                new ServiceHierarchy
                {
                    AccountId = _parterAccountId2,
                    ParentAccId = _acmeAccountId
                }
            };
        }

        private List<ServiceHierarchy> CreatePartner1Child()
        {
            return new List<ServiceHierarchy>
            {
                new ServiceHierarchy
                {
                    AccountId = _partner1CustomerAccountId1,
                    ParentAccId = _parterAccountId1
                },
                new ServiceHierarchy
                {
                    AccountId = _partner1CustomerAccountId2,
                    ParentAccId = _parterAccountId1
                }
            };
        }

        private List<ServiceHierarchy> CreatePartner2Child()
        {
            return new List<ServiceHierarchy>
            {
                new ServiceHierarchy
                {
                    AccountId = _partner2CustomerAccountId1,
                    ParentAccId = _parterAccountId2
                }
            };
        }

        private Mapper CreateMapper()
        {
            var profile = new ServiceHierarchyProfile();
            var configuration = new MapperConfiguration(config => config.AddProfile(profile));
            return new Mapper(configuration);
        }

        private HttpClient CreateHttpClientStub(MockHttpMessageHandler handler)
        {
            var httpClient = new HttpClient(handler);
            httpClient.BaseAddress = new Uri(_baseAddress);
            return httpClient;
        }
    }
}
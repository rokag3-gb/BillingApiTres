using AutoMapper;
using Azure.Core;
using Billing.Data.Interfaces;
using Billing.Data.Models;
using Billing.EF.Repositories;
using BillingApiTres.Controllers.Tenants;
using BillingApiTres.Models.Clients;
using BillingApiTres.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCaching;
using System.Collections.Generic;

namespace BillingApiTres.Controllers.ServiceHierachies
{
    [Route("[controller]")]
    [Authorize]
    public class GetServiceHierachyController(
        IServiceHierarchyRepository serviceHierachyRepository,
        IMapper mapper,
        SalesClient salesClient,
        ILogger<GetTenantController> logger) : ControllerBase
    {
        private enum AccountType
        {
            Acme,
            Partner,
            Customer,
            None
        }

        [HttpGet("/service-organizations/{serialNo}")]
        public async Task<ServiceHierarchyResponse?> Get(int serialNo)
        {
            var response = await serviceHierachyRepository.Get(serialNo);
            if (response == null)
                return null;

            var token = HttpContext.Request.Headers.Authorization.ToString();
            if (token.Split(" ").Count() >= 2)
                token = token.Split(" ")[1];

            SalesAccount? parentAccount = default;
            if (response.ParentAccId > 0)
                parentAccount = await salesClient.Get<SalesAccount>($"account/{response.ParentAccId}", token!);
            var account = await salesClient.Get<SalesAccount>($"account/{response.AccountId}", token!);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ServiceHierarchy, ServiceHierarchyResponse>()
                    .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId))
                    .ForMember(dest => dest.SerialNo, opt => opt.MapFrom(src => src.Sno))
                    .ForMember(dest => dest.RealmName, opt => opt.MapFrom(src => src.Tenant.RealmName))
                    .ForMember(dest => dest.ContractorId, opt => opt.MapFrom(src => src.ParentAccId))
                    .ForMember(dest => dest.ContractorName, opt => opt.MapFrom((src => parentAccount != null ? parentAccount.AccountName : string.Empty)))
                    .ForMember(dest => dest.ContracteeId, opt => opt.MapFrom(src => src.AccountId))
                    .ForMember(dest => dest.ContracteeName, opt => opt.MapFrom(src => account.AccountName))
                    .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                    .ForMember(dest => dest.ContractDate, opt => opt.MapFrom(src => src.StartDate))
                    .ForMember(dest => dest.ExpireDate, opt => opt.MapFrom(src => src.EndDate));
            });

            return config.CreateMapper().Map<ServiceHierarchyResponse>(response);
        }

        [HttpGet("/service-organizations/{accountId}/hierachy")]
        public async Task<List<ServiceHierarchyResponse>> GetList(int accountId)
        {
            var parent = await serviceHierachyRepository.GetParent(accountId);
            if (parent == null)
            {
                logger.LogError($"계약 공급 업체를 찾을 수 없음 : account id - {accountId}");
                return new List<ServiceHierarchyResponse>();
            }

            var token = HttpContext.Request.Headers.Authorization.ToString();
            if (token.Split(" ").Count() >= 2)
                token = token.Split(" ")[1];

            var accounts = await salesClient.Get<List<SalesAccount>>("account?limit=99999", token!);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ServiceHierarchy, ServiceHierarchyResponse>()
                    .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId))
                    .ForMember(dest => dest.SerialNo, opt => opt.MapFrom(src => src.Sno))
                    .ForMember(dest => dest.RealmName, opt => opt.MapFrom(src => src.Tenant.RealmName))
                    .ForMember(dest => dest.ContractorId, opt => opt.MapFrom(src => src.ParentAccId))
                    .ForMember(dest => dest.ContractorName, opt => opt.MapFrom<string?>((s, d) =>
                    {
                        return accounts.Where(a => a.AccountId == s.ParentAccId)?.FirstOrDefault()?.AccountName ?? "";
                    }))
                    .ForMember(dest => dest.ContracteeId, opt => opt.MapFrom(src => src.AccountId))
                    .ForMember(dest => dest.ContracteeName, opt => opt.MapFrom<string>((s, d) =>
                    {
                        return accounts.Where(a => a.AccountId == s.AccountId).FirstOrDefault()?.AccountName ?? "";
                    }))
                    .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                    .ForMember(dest => dest.ContractDate, opt => opt.MapFrom(src => src.StartDate))
                    .ForMember(dest => dest.ExpireDate, opt => opt.MapFrom(src => src.EndDate));
            });

            var mapper = config.CreateMapper();

            AccountType accountType = AccountType.None;
            if (parent.ParentAccId == 0)
                accountType = AccountType.Acme;
            else if (parent.ParentAccId == 1)
                accountType = AccountType.Partner;
            else
                accountType = AccountType.Customer;

            if (accountType == AccountType.Acme)
            {
                var responses = new List<ServiceHierarchyResponse>();

                responses.Add(mapper.Map<ServiceHierarchyResponse>(parent));
                var partners = await serviceHierachyRepository.GetChild(accountId);

                var res = await MapMany(mapper, partners);
                responses.AddRange(res);

                return responses;
            }
            else if (accountType == AccountType.Partner)
            {
                var responses = new List<ServiceHierarchyResponse>();
                responses.AddRange(ConfigureParent(mapper, parent));
                var customers = await serviceHierachyRepository.GetChild(accountId);
                responses.AddRange(customers.Select(c =>  mapper.Map<ServiceHierarchyResponse>(c)));

                return responses;
            }
            else
            {
                return ConfigureParent(mapper, parent);
            }
        }

        private List<ServiceHierarchyResponse> ConfigureParent(IMapper mapper, ServiceHierarchy item)
        {
            var responses = new List<ServiceHierarchyResponse>();

            var parentId = item.ParentAccId;
            var accId = item.AccountId;

            item.AccountId = item.ParentAccId;
            item.ParentAccId = 0;
            
            responses.Add(mapper.Map<ServiceHierarchyResponse>(item));

            item.ParentAccId = parentId;
            item.AccountId = accId;
            responses.Add(mapper.Map<ServiceHierarchyResponse>(item));

            return responses;
        }


        private async Task<List<ServiceHierarchyResponse>> MapMany(IMapper mapper, List<ServiceHierarchy> source)
        {
            List<ServiceHierarchyResponse> responses = new();
            foreach (var sourceItem in source)
            {
                responses.Add(mapper.Map<ServiceHierarchyResponse>(sourceItem));
                var customers = await serviceHierachyRepository.GetChild(sourceItem.AccountId);
                responses.AddRange(customers.Select(c => mapper.Map<ServiceHierarchyResponse>(c)));
            }
            return responses;
        }
    }
}

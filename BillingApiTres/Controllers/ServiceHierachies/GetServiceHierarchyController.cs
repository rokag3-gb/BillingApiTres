using AutoMapper;
using Azure.Core;
using Billing.Data.Interfaces;
using Billing.Data.Models;
using Billing.EF.Repositories;
using BillingApiTres.Controllers.Tenants;
using BillingApiTres.Converters;
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
        ILogger<GetServiceHierachyController> logger) : ControllerBase
    {
        private enum AccountType
        {
            Acme,
            Partner,
            Customer,
            None
        }

        [HttpGet("/service-organizations/{serialNo}")]
        public async Task<ActionResult<ServiceHierarchyResponse>> Get(long serialNo)
        {
            var response = await serviceHierachyRepository.Get(serialNo);
            if (response == null)
                return NotFound(new { serialNo = serialNo });

            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);

            SalesAccount? parentAccount = default;
            if (response.ParentAccId > 0)
                parentAccount = await salesClient.Get<SalesAccount>($"account/{response.ParentAccId}", token?.RawData!);
            var account = await salesClient.Get<SalesAccount>($"account/{response.AccountId}", token?.RawData!);
            var list = new List<SalesAccount> { parentAccount ?? new(), account };

            return mapper.Map<ServiceHierarchyResponse>(response, options =>
            {
                options.Items["accounts"] = list;
            });
        }

        [HttpGet("/service-organizations/{accountId}/hierarchy")]
        public async Task<ActionResult<List<ServiceHierarchyResponse>>> GetList(long accountId)
        {
            var parent = await serviceHierachyRepository.GetParent(accountId);
            if (parent == null)
            {
                logger.LogError($"계약 공급 업체를 찾을 수 없음 : account id - {accountId}");
                return NoContent();
            }

            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);
            var accounts = await salesClient.Get<List<SalesAccount>>("account?limit=99999", token?.RawData!);
            
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

                responses.Add(mapper.Map<ServiceHierarchyResponse>(parent, options =>
                {
                    options.Items["accounts"] = accounts;
                }));
                var partners = await serviceHierachyRepository.GetChild(accountId);

                var res = await MapMany(mapper, partners, accounts);
                responses.AddRange(res);

                return responses;
            }
            else if (accountType == AccountType.Partner)
            {
                var responses = new List<ServiceHierarchyResponse>();
                responses.AddRange(ConfigureParent(mapper, parent, accounts));
                var customers = await serviceHierachyRepository.GetChild(accountId);
                responses.AddRange(customers.Select(c =>  mapper.Map<ServiceHierarchyResponse>(c, options =>
                {
                    options.Items["accounts"] = accounts;
                })));

                return responses;
            }
            else
            {
                return ConfigureParent(mapper, parent, accounts);
            }
        }

        private List<ServiceHierarchyResponse> ConfigureParent(IMapper mapper, ServiceHierarchy item, List<SalesAccount> accounts)
        {
            var responses = new List<ServiceHierarchyResponse>();

            var parentId = item.ParentAccId;
            var accId = item.AccountId;

            item.AccountId = item.ParentAccId;
            item.ParentAccId = 0;
            
            responses.Add(mapper.Map<ServiceHierarchyResponse>(item, options =>
            {
                options.Items["accounts"] = accounts;
            }));

            item.ParentAccId = parentId;
            item.AccountId = accId;
            responses.Add(mapper.Map<ServiceHierarchyResponse>(item, options =>
            {
                options.Items["accounts"] = accounts;
            }));

            return responses;
        }


        private async Task<List<ServiceHierarchyResponse>> MapMany(IMapper mapper, List<ServiceHierarchy> source, List<SalesAccount> accounts)
        {
            List<ServiceHierarchyResponse> responses = new();
            foreach (var sourceItem in source)
            {
                responses.Add(mapper.Map<ServiceHierarchyResponse>(sourceItem, options =>
                {
                    options.Items["accounts"] = accounts;
                }));
                var customers = await serviceHierachyRepository.GetChild(sourceItem.AccountId);
                responses.AddRange(customers.Select(c => mapper.Map<ServiceHierarchyResponse>(c, options =>
                {
                    options.Items["accounts"] = accounts;
                })));
            }
            return responses;
        }
    }
}

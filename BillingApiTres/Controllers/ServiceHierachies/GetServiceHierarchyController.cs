using AutoMapper;
using Billing.Data.Interfaces;
using Billing.Data.Models;
using Billing.Data.Models.Sale;
using BillingApiTres.Converters;
using BillingApiTres.Models.Clients;
using BillingApiTres.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using System.IdentityModel.Tokens.Jwt;

namespace BillingApiTres.Controllers.ServiceHierachies
{
    [Route("[controller]")]
    [Authorize]
    public class GetServiceHierachyController(
        IServiceHierarchyRepository serviceHierachyRepository,
        IAccountKeyRepository accountKeyRepository,
        IMapper mapper,
        AcmeGwClient gwClient,
        IConfiguration config,
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
                parentAccount = await gwClient.Get<SalesAccount>($"sales/account/{response.ParentAccId}", token?.RawData!);
            var account = await gwClient.Get<SalesAccount>($"sales/account/{response.AccountId}", token?.RawData!);
            var list = new List<SalesAccount> { parentAccount ?? new(), account };

            parentAccount.AccountId = 0;
            var accountKeys = await accountKeyRepository.GetKeyList(new List<long> { account.AccountId });

            var accountLinks = await gwClient.Get<List<AccountLink>>($"sales/accountLink?limit=999999&offset=0&accountIdCsv={account.AccountId}", token?.RawData!);
            var accountUsers = await gwClient.Get<List<AccountUser>>($"sales/accountUser?limit=999999&offset=0&accountIdCsv={account.AccountId}", token?.RawData!);

            return mapper.Map<ServiceHierarchyResponse>(response, options =>
            {
                options.Items["accounts"] = list;
                options.Items["accountKeys"] = accountKeys;
                options.Items["accountLink"] = accountLinks;
                options.Items["accountUser"] = accountUsers;
                options.Items["timezone"] =
                    HttpContext.Request.Headers[$"{config.GetValue<string>("TimezoneHeader")}"];
            });
        }

        [HttpGet("/service-organizations/{accountId}/hierarchy")]
        public async Task<ActionResult<List<ServiceHierarchyResponse>>> GetList(string accountId)
        {
#if DEBUG
            AccountKey account = new AccountKey();

            if (long.TryParse(accountId, out long aid) && accountId.Length != 7)
                account.AccountId = aid;
            else
                account = await accountKeyRepository.GetId(accountId);
#else
            var account = await accountKeyRepository.GetId(accountId);
#endif
            var parent = await serviceHierachyRepository.GetParent(account.AccountId);
            if (parent == null)
            {
                logger.LogError($"계약 공급 업체를 찾을 수 없음 : account id - {accountId}");
                return NoContent();
            }

            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);
            
            AccountType accountType = AccountType.None;
            if (parent.ParentAccId == 0)
                accountType = AccountType.Acme;
            else if (parent.ParentAccId == 1)
                accountType = AccountType.Partner;
            else
                accountType = AccountType.Customer;

            parent.ParentAccId = 0; //상위 계약 업체 정보 노출 방지

            if (accountType == AccountType.Acme)
            {
                var partners = await serviceHierachyRepository.GetChild(account.AccountId);
                var customers = await serviceHierachyRepository
                    .GetChild(partners.Select(p => p.AccountId).ToList());

                List<ServiceHierarchy> list = [parent, .. partners, .. customers];

                return await MapResponse(list, token);
            }
            else if (accountType == AccountType.Partner)
            {
                var customers = await serviceHierachyRepository.GetChild(account.AccountId);

                List<ServiceHierarchy> list = [parent, .. customers];

                var accountKeys = await accountKeyRepository.GetKeyList(list.Select(a => a.AccountId).ToList());

                return await MapResponse(list, token);
            }
            else
            {
                return await MapResponse(new List<ServiceHierarchy> { parent }, token);
            }
        }

        private async Task<List<ServiceHierarchyResponse>> MapResponse(List<ServiceHierarchy> list, JwtSecurityToken token)
        {
            var ids = list.SelectMany(a => new[] { a.ParentAccId, a.AccountId }).Distinct().ToList();
            var accounts = await gwClient
                .Get<List<SalesAccount>>($"sales/account?limit=999999&accountIds={string.Join(",", ids)}",
                                         token?.RawData!);

            var accountLinks = await gwClient
                .Get<List<AccountLink>>($"sales/accountLink?limit=999999&offset=0&accountIdCsv={string.Join(",", list.Select(a => a.AccountId))}",
                                        token?.RawData!);

            var accountUsers = await gwClient
                .Get<List<AccountUser>>($"sales/accountUser?limit=999999&offset=0&accountIdCsv={string.Join(",", list.Select(a => a.AccountId))}",
                                        token?.RawData!);

            var accountKeys = await accountKeyRepository
                .GetKeyList(ids);

            return mapper.Map<List<ServiceHierarchyResponse>>(list, opt =>
            {
                opt.Items["accounts"] = accounts;
                opt.Items["accountKeys"] = accountKeys;
                opt.Items["accountLink"] = accountLinks;
                opt.Items["accountUser"] = accountUsers;
                opt.Items["timezone"] = 
                    HttpContext.Request.Headers[$"{config.GetValue<string>("TimezoneHeader")}"];
            });
        }
    }
}

using AutoMapper;
using Billing.Data.Interfaces;
using Billing.Data.Models.Iam;
using BillingApiTres.Converters;
using BillingApiTres.Models.Clients;
using BillingApiTres.Models.Dto;
using BillingApiTres.Models.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;
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
            var accountIds = HttpContext.Items[config["AccountHeader"]!] as ImmutableHashSet<long>;

            var response = await serviceHierachyRepository.Get(serialNo);
            if (response == null)
                return NotFound(new { serialNo = serialNo });

            if (accountIds?.Contains(response.AccountId) == false)
                return Forbid();

            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);

            SalesAccount? parentAccount = default;
            if (response.ParentAccId > 0)
                parentAccount = await gwClient.Get<SalesAccount>($"sales/account/{response.ParentAccId}", token?.RawData!);
            var account = await gwClient.Get<SalesAccount>($"sales/account/{response.AccountId}", token?.RawData!);

            if (account == null)
            {
                logger.LogError($"대상 Account가 존재하지 않습니다 - ServiceHierarchy.AccountId : {response.AccountId}");
                return NotFound(new { serialNo = serialNo });
            }

            var list = new List<SalesAccount> { parentAccount ?? new(), account };

            var accountKeys = await accountKeyRepository.GetKeyList(new List<long> { account.AccountId });

            var accountLinks = await gwClient.Get<List<AccountLink>>($"sales/accountLink?limit=999999&offset=0&accountIdCsv={account.AccountId}", token?.RawData!);
            var accountUsers = await gwClient.Get<List<AccountUser>>($"sales/accountUser?limit=999999&offset=0&accountIdCsv={account.AccountId}", token?.RawData!);

            return mapper.Map<ServiceHierarchyResponse>(response, options =>
            {
                options.Items["accounts"] = list;
                options.Items["accountLink"] = accountLinks;
                options.Items["accountUser"] = accountUsers;
                options.Items["timezone"] =
                    HttpContext.Request.Headers[$"{config.GetValue<string>("TimezoneHeader")}"];
            });
        }

        [AuthorizeAccountIdFilter([nameof(accountId)])]
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
            
            AccountType accountType = AccountType.None;
            if (parent.ParentAccId == 0)
                accountType = AccountType.Acme;
            else if (parent.ParentAccId == 1)
                accountType = AccountType.Partner;
            else
                accountType = AccountType.Customer;

            if (accountType == AccountType.Acme)
            {
                var partners = await serviceHierachyRepository.GetChild(accountId);
                var customers = await serviceHierachyRepository
                    .GetChild(partners.Select(p => p.AccountId).ToList());

                List<ServiceHierarchy> list = [parent, .. partners, .. customers];

                return await MapResponse(list, token);
            }
            else if (accountType == AccountType.Partner)
            {
                var customers = await serviceHierachyRepository.GetChild(accountId);

                List<ServiceHierarchy> list = [parent, .. customers];

                return await MapResponse(list, token);
            }
            else
            {
                return await MapResponse(new List<ServiceHierarchy> { parent }, token);
            }
        }

        [HttpGet("/service-organizations/noncontracts")]
        public async Task<ActionResult<List<SalesAccount>>> GetNoncontractAccounts(
            [FromQuery] NonContractAccountRequest request)
        {
            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);

            var allContracts = await serviceHierachyRepository.All(request.Offset, request.Limit);
            var allContractedIds = allContracts.Select(a => a.AccountId);

            var accounts = await gwClient
                .Get<List<SalesAccount>>($"sales/account?limit=999999",
                                         token?.RawData!);

            accounts.RemoveAll(a => allContractedIds.Contains(a.AccountId));

            return accounts;
        }

        private async Task<List<ServiceHierarchyResponse>> MapResponse(List<ServiceHierarchy> list, JwtSecurityToken token)
        {
            var ids = list.SelectMany(a => new[] { a.ParentAccId, a.AccountId }).Distinct().ToList();
            var accounts = await gwClient
                .Get<List<SalesAccount>>($"sales/account?limit=999999&accountIds={string.Join(",", ids)}",
                                         token?.RawData!);
            var accountIds = accounts.Select(a => a.AccountId).ToHashSet();
            list = list.Where(sh => accountIds.Contains(sh.AccountId))
                       .Where(sh => sh.ParentAccId == 0 || accountIds.Contains(sh.ParentAccId))
                       .ToList();

            var accountLinks = await gwClient
                .Get<List<AccountLink>>($"sales/accountLink?limit=999999&offset=0&accountIdCsv={string.Join(",", list.Select(a => a.AccountId))}",
                                        token?.RawData!);

            var accountUsers = await gwClient
                .Get<List<AccountUser>>($"sales/accountUser?limit=999999&offset=0&accountIdCsv={string.Join(",", list.Select(a => a.AccountId))}",
                                        token?.RawData!);

            return mapper.Map<List<ServiceHierarchyResponse>>(list, opt =>
            {
                opt.Items["accounts"] = accounts;
                opt.Items["accountLink"] = accountLinks;
                opt.Items["accountUser"] = accountUsers;
                opt.Items["timezone"] = 
                    HttpContext.Request.Headers[$"{config.GetValue<string>("TimezoneHeader")}"];
            });
        }
    }
}

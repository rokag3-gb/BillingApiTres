using AutoMapper;
using Billing.Data.Interfaces;
using Billing.Data.Models.Bill;
using Billing.Data.Models.Iam;
using BillingApiTres.Converters;
using BillingApiTres.Extensions;
using BillingApiTres.Models.Clients;
using BillingApiTres.Models.Configurations;
using BillingApiTres.Models.Dto;
using BillingApiTres.Models.Dto.Bills;
using BillingApiTres.Models.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Validations;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;

namespace BillingApiTres.Controllers.Bills
{
    [Route("[controller]")]
    [Authorize]
    public class BillController(IBillRepository billRepository,
                                INcpRepository ncpRepository,
                                IServiceHierarchyRepository serviceHierarchyRepository,
                                IProductRepository productRepository,
                                AcmeGwClient gwClient,
                                ITimeZoneConverter timeZoneConverter,
                                IMapper mapper,
                                IConfiguration config,
                                CurrencyConverter currencyConverter,
                                ILogger<BillController> logger) : ControllerBase
    {
        private ImmutableHashSet<Product>? _products;

        /// <summary>
        /// 지정한 기간 내 특정 고객사들의 청구서를 조회합니다
        /// </summary>
        [AuthorizeAccountIdFilter([nameof(request.AccountIds)])]
        [HttpGet("/bills")]
        public async Task<ActionResult<List<BillResponse>>> GetList([FromQuery] BillListRequest request)
        {
            if (request.From.CompareTo(request.To) > 0)
                return BadRequest($"검색 기간 설정 오류 : {request.From} ~ {request.To}");

            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);
            var tz = HttpContext.Request.Headers[$"{config.GetValue<string>("TimezoneHeader")}"];

            var accountIds = request.AccountIds
                .Split(",", StringSplitOptions.TrimEntries)
                .Select(s =>
                {
                    if (long.TryParse(s, out long id))
                        return id;
                    return -1;
                })
                .Where(i => i >= 1).ToList();

            //get bills
            var bills = billRepository.GetRange(timeZoneConverter.ConvertToUtc(request.From, tz!),
                                                timeZoneConverter.ConvertToUtc(request.To, tz!),
                                                accountIds,
                                                null,
                                                request.Offset,
                                                request.limit);

            //get status codes
            var codes = new List<SaleCode>();
            var requestCodeKinds = bills.Select(b => b.StatusCode.Split("-").First()).Distinct();

            foreach (var kind in requestCodeKinds)
            {
                var ret = await gwClient.Get<List<SaleCode>>($"sales/code/{kind}/childs", token?.RawData!);
                codes.AddRange(ret);
            }

            //get currency codes
            var currencyInfos = bills.Select(b => b.CurrencyCode)
                .Distinct()
                .Select(currencyConverter.GetCurrencyInfo)
                .Where(c => c != null);

            //get userentity
            var userIds = bills
                .SelectMany(b => new[] { b.SellerManagerId, b.BuyerManagerId, b.SaverId })
                .Distinct()
                .Where(s => string.IsNullOrEmpty(s) == false);
            var users = await gwClient.Get<List<IamUserEntity>>($"iam/users?ids={string.Join(",", userIds)}", token?.RawData!);

            //get accounts
            var usedAccountIds = bills.SelectMany(b => new[] { b.SellerAccountId, b.BuyerAccountId, b.ConsumptionAccountId }).Distinct().Where(a => a != null).Cast<long>();
            var accounts = await gwClient.Get<List<SalesAccount>>($"sales/account?limit=999999&accountIds={string.Join(",", usedAccountIds)}", token?.RawData!);

            var response = bills.Select(b =>
            {
                return mapper.Map<BillResponse>(b, opt =>
                {
                    opt.AfterMap((o, br) =>
                    {
                        var currencyInfo = currencyInfos.FirstOrDefault(c => c?.CurrencyCode == b.CurrencyCode);

                        br.SellerAccountName = accounts?.FirstOrDefault(a => a.AccountId == b.SellerAccountId)?.AccountName ?? string.Empty;
                        //br.BuyerAccountId = usedAccountKeys?.FirstOrDefault(a => a.AccountId == b.BuyerAccountId)?.AccountKey1 ?? string.Empty;
                        br.BuyerAccountName = accounts?.FirstOrDefault(a => a.AccountId == b.BuyerAccountId)?.AccountName ?? string.Empty;
                        //br.ConsumptionAccountId = usedAccountKeys?.FirstOrDefault(a => a.AccountId == b.ConsumptionAccountId)?.AccountKey1 ?? string.Empty;
                        br.ConsumptionAccountName = accounts?.FirstOrDefault(a => a.AccountId == b.ConsumptionAccountId)?.AccountName ?? string.Empty;
                        br.StatusName = codes?.FirstOrDefault(c => c.Code == b.StatusCode)?.Name ?? string.Empty;
                        br.CurrencyName = currencyInfo?.CurrencyEnglishName ?? string.Empty;
                        br.CurrencySymbol = currencyInfo?.CurrencySymbol ?? string.Empty;
                        br.BuyerManageName = users?.FirstOrDefault(u => u.Id == b.BuyerManagerId)?.Name ?? string.Empty;
                        br.SellerManageName = users?.FirstOrDefault(u => u.Id == b.SellerManagerId)?.Name ?? string.Empty;
                        br.SaverName = users?.FirstOrDefault(u => u.Id == b.SaverId)?.Name ?? string.Empty;
                        br.BillDate = timeZoneConverter.ConvertToLocal(b.BillDate, tz);
                        if (b.ConsumptionStartDate.HasValue)
                            br.ConsumptionStartDate = timeZoneConverter.ConvertToLocal(b.ConsumptionStartDate.Value, tz);
                        if (b.ConsumptionEndDate.HasValue)
                            br.ConsumptionEndDate = timeZoneConverter.ConvertToLocal(b.ConsumptionEndDate.Value, tz);
                        br.SavedAt = timeZoneConverter.ConvertToLocal(b.SavedAt, tz);
                    });
                });
            }).ToList();

            return response;
        }

        /// <summary>
        /// 특정 청구서들의 상태를 갱신합니다
        /// </summary>
        [HttpPut("/bills/status")]
        public async Task<ActionResult> Update([FromBody] BillUpdateRequest request)
        {
            var bills = billRepository.GetRange(null, null, null, request.BillIds, null, null);

            if (bills?.Any() == false)
                return NotFound($"not found bill resource : {string.Join(",", request.BillIds)}");

            if (HttpContext.AuthenticateAccountId(bills!.Select(b => b.BuyerAccountId)) == false)
                return Forbid();

            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);
            var billStatus = await gwClient.Get<List<SaleCode>>($"sales/code/BST/childs", token?.RawData!);
            var billStatusCodes = billStatus.Select(bs => bs.Code).ToHashSet();

            if (billStatusCodes.Contains(request.StatusCode) == false)
                return BadRequest($"적절하지 않은 상태 코드 요청 입니다 : {request.StatusCode} - {string.Join(", ", billStatusCodes)} 만 유효합니다");

            billRepository.UpdateStatus(request.StatusCode, request.BillIds);

            return Ok();
        }

        /// <summary>
        /// 대상 청구서를 고객용 청구서로 생성합니다
        /// </summary>
        /// <response code="400">미확정 상태인 청구서를 생성하는 경우</response>
        /// <response code="424">고객의 계약 관련 상품(Bill.dbo.Product)의 레코드가 존재하지 않거나 찾을 수 없는 경우</response>
        /// <response code="422">고객용으로 생성된 청구서를 다시 생성하려고 하는 경우</response>
        /// <response code="409">같은 월, 같은 고객, 같은 청구 금액으로 청구서가 이미 생성되어 있음</response>
        [HttpPost("/bills/customer")]
        public async Task<ActionResult<MultiHttpCodeResponse<BillResponse>>> CreateCustomerBills([FromBody] CustomerBillCreateRequest request)
        {
            var bills = billRepository
                .GetRangeWithRelations(null,
                                       null,
                                       null,
                                       request.BillIds,
                                       null,
                                       null,
                                       true);

            var accountIds = bills.Select(b => b.ConsumptionAccountId)
                                      .Where(a => a.HasValue)
                                      .Select(a => a!.Value)
                                      .Distinct()
                                      .ToHashSet();

            if (HttpContext.AuthenticateAccountId(accountIds) == false)
                return Forbid();

            var response = new MultiHttpCodeResponse<BillResponse>();
            var billStatusCode = config.GetSection(nameof(BillStatusCode)).Get<BillStatusCode>() ?? new BillStatusCode();

            var uncertainedBills = bills.Where(b => b.StatusCode == billStatusCode!.Uncertain).ToList();
            if (uncertainedBills.Any())
            {
                uncertainedBills.ForEach(b => bills.Remove(b));
                response.AddFail(StatusCodes.Status400BadRequest, uncertainedBills.Select(bi => bi.BillId));

                if (bills.Any() == false)
                    return response;
            }

            var publishedBills = bills.Where(b => b.BuyerAccountId == b.ConsumptionAccountId).ToList();
            if (publishedBills.Any())
            {
                publishedBills.ForEach(pb => bills.Remove(pb));
                response.AddFail(StatusCodes.Status422UnprocessableEntity, publishedBills.Select(b => b.BillId));

                logger.LogInformation($"고객용으로 생성된 청구서는 다시 발급할 수 없습니다 : " +
                    $"BillId({string.Join(",", publishedBills.Select(pb => pb.BillId))})");

                if (bills.Any() == false)
                    return response;
            }

            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);
            var tz = HttpContext.Request.Headers[$"{config.GetValue<string>("TimezoneHeader")}"];

            //get status codes
            var codes = new List<SaleCode>();
            var requestCodeKinds = bills.Select(b => b.StatusCode.Split("-").First()).Distinct();

            foreach (var kind in requestCodeKinds)
            {
                var ret = await gwClient.Get<List<SaleCode>>($"sales/code/{kind}/childs", token?.RawData!);
                codes.AddRange(ret);
            }

            //get userentity
            var userIds = bills
                .SelectMany(b => new[] { b.SellerManagerId, b.BuyerManagerId, b.SaverId })
                .Distinct()
                .Where(s => string.IsNullOrEmpty(s) == false);
            var users = await gwClient.Get<List<IamUserEntity>>($"iam/users?ids={string.Join(",", userIds)}", token?.RawData!);

            var accountTypeCode = config.GetSection(nameof(AccountTypeCode)).Get<AccountTypeCode>() ?? new AccountTypeCode();
            var serviceHierarchies = serviceHierarchyRepository.GetList(accountIds, [accountTypeCode.Acme]).ToHashSet();
            var acmeAccountId = serviceHierarchies.FirstOrDefault(s => s.TypeCode == accountTypeCode.Acme)?.AccountId ?? 1;
            var accounts = await gwClient.Get<List<SalesAccount>>($"sales/account?limit=999999&accountIds={string.Join<long>(",", [.. accountIds, .. new List<long> { acmeAccountId }])}", token?.RawData!);

            var copiedBills = bills.Select(b => mapper.Map<Bill>(b,
                    opt => opt.AfterMap((s, d) =>
                    {
                        d.SellerAccountId = acmeAccountId;
                        d.BuyerAccountId = d.ConsumptionAccountId!.Value;
                        d.StatusCode = billStatusCode?.Uncertain ?? "BST-001";
                        d.SellerManagerId = accounts.FirstOrDefault(a => a.AccountId == acmeAccountId)?.ManagerId;
                        d.BuyerManagerId = accounts.FirstOrDefault(a => a.AccountId == d.ConsumptionAccountId)?.ManagerId;
                        d.SaverId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        foreach (var billItem in d.BillItems)
                        {
                            billItem.Description = billItem.Product?.ProductName;
                            billItem.SaverId = d.SaverId;
                        }
                    }))).ToHashSet() ?? [];

            _products = ImmutableHashSet.Create(productRepository.GetList().ToArray());

            var partnerAccountIds = serviceHierarchies
                .Where(s => s.TypeCode == accountTypeCode.Partner)
                .Select(s => s.AccountId);

            var customerAccountIds = serviceHierarchies
                .Where(s => s.TypeCode == accountTypeCode.Customer)
                .Select(s => s.AccountId);

            copiedBills = copiedBills.Join(serviceHierarchies,
                b => b.ConsumptionAccountId!.Value,
                s => s.AccountId,
                (b, s) => { b.ServiceHierarchyConfigs = s.ServiceHierarchyConfigs; return b; }).ToHashSet();

            var customerBills = copiedBills
                .Where(b => b.ConsumptionAccountId != null)
                .Where(b => customerAccountIds.Contains(b.ConsumptionAccountId!.Value));

            foreach (var customerBill in customerBills)
            {
                customerBill.BuyerAccountId = customerBill.ConsumptionAccountId!.Value;
            }

            var partnerBills = copiedBills
                .Where(b => b.ConsumptionAccountId != null)
                .Where(b => partnerAccountIds.Contains(b.ConsumptionAccountId!.Value));


            ///고객용 청구서 생성 로직
            var billItemByVendor = customerBills
                .SelectMany(b => b.BillItems)
                .GroupBy(bi => bi.VendorCode);

            foreach (var billItems in billItemByVendor)
            {
                switch (billItems.Key)
                {
                    case "VEN-NCP":
                        var ncpResponse = CustomerNcpServices(billItems.ToList());
                        response.AddFail(ncpResponse.Fails);
                        break;
                    case "VEN-AWS":
                        //aws 사용 고객 청구서 생성
                        break;
                    default:
                        break;
                }
            }

            //todo : 파트너 대상 청구서 발급은 별도의 로직으로 작동한다. 


            var resultCheckDuplicate = CheckDulplicateBill(copiedBills);
            response.AddFail(resultCheckDuplicate.Fails);

            var failedBillIds = response.Fails.SelectMany(f => f.EntityIds).ToList();
            var failedBills = copiedBills.Where(cb => failedBillIds.Contains(cb.OriginalBillId)).ToList();
            failedBills.ForEach(fb => copiedBills.Remove(fb));

            var addedBills = billRepository.Create(copiedBills);

            var currencyInfos = addedBills.Select(b => b.CurrencyCode)
                .Distinct()
                .Select(currencyConverter.GetCurrencyInfo)
                .Where(c => c != null);

            var billResponse = addedBills.Select(b => mapper.Map<BillResponse>(b, opt =>
            {
                opt.AfterMap((s, br) =>
                {
                    var currencyInfo = currencyInfos.FirstOrDefault(c => c?.CurrencyCode == b.CurrencyCode);

                    br.SellerAccountName = accounts?.FirstOrDefault(a => a.AccountId == b.SellerAccountId)?.AccountName ?? string.Empty;
                    br.BuyerAccountName = accounts?.FirstOrDefault(a => a.AccountId == b.BuyerAccountId)?.AccountName ?? string.Empty;
                    br.ConsumptionAccountName = accounts?.FirstOrDefault(a => a.AccountId == b.ConsumptionAccountId)?.AccountName ?? string.Empty;
                    br.StatusName = codes?.FirstOrDefault(c => c.Code == b.StatusCode)?.Name ?? string.Empty;
                    br.CurrencyName = currencyInfo?.CurrencyEnglishName ?? string.Empty;
                    br.CurrencySymbol = currencyInfo?.CurrencySymbol ?? string.Empty;
                    br.BuyerManageName = users?.FirstOrDefault(u => u.Id == b.BuyerManagerId)?.Name ?? string.Empty;
                    br.SellerManageName = users?.FirstOrDefault(u => u.Id == b.SellerManagerId)?.Name ?? string.Empty;
                    br.SaverName = users?.FirstOrDefault(u => u.Id == b.SaverId)?.Name ?? string.Empty;
                    br.BillDate = timeZoneConverter.ConvertToLocal(b.BillDate, tz!);
                    if (b.ConsumptionStartDate.HasValue)
                        br.ConsumptionStartDate = timeZoneConverter.ConvertToLocal(b.ConsumptionStartDate.Value, tz!);
                    if (b.ConsumptionEndDate.HasValue)
                        br.ConsumptionEndDate = timeZoneConverter.ConvertToLocal(b.ConsumptionEndDate.Value, tz!);
                    br.SavedAt = timeZoneConverter.ConvertToLocal(b.SavedAt, tz!);
                });
            }));

            response.Success.AddRange(billResponse);

            return Ok(response);
        }

        /// <summary>
        /// 대상 청구서를 삭제합니다.
        /// </summary>
        /// <response code="400">확정 상태인 청구서를 삭제하는 경우 / 요청 인자에 bill id가 없는 경우 / 요청한 사용자의 account가 확인되지 않는 경우</response>
        /// <response code="422">원청구서를 삭제하는 경우</response>
        [HttpDelete("/bills")]
        public async Task<ActionResult<MultiHttpCodeResponse<long>>> DeleteBill([FromBody] BillDeleteRequest request)
        {
            var response = new MultiHttpCodeResponse<long>();

            if (request.BillIds.Any() == false)
                return BadRequest("Bill Id 값이 비었습니다.");

            var bills = billRepository
                .GetRangeWithRelations(null,
                                       null,
                                       null,
                                       request.BillIds,
                                       null,
                                       null,
                                       false,
                                       true);

            var accountIds = bills.SelectMany(b => new[] { b.BuyerAccountId, b.ConsumptionAccountId })
                                      .Where(a => a.HasValue)
                                      .Select(a => a!.Value)
                                      .Distinct()
                                      .ToHashSet();

            if (HttpContext.AuthenticateAccountId(accountIds) == false)
                return Forbid();

            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);
            var accountUsers = await gwClient.Get<List<AccountUser>>($"sales/accountUser?userUuid={token!.Subject}", token!.RawData);

            if (accountUsers == null || accountUsers.Any() == false)
                return BadRequest("사용자의 account가 확인되지 않습니다");

            //이미 삭제된 청구서 처리
            var deletedBills = bills.Where(b => b.IsDelete == true).ToList();

            if (bills.Count == deletedBills.Count)
            {
                response.Success.AddRange(request.BillIds);
                return Ok(response);
            }
            deletedBills.ForEach(b => bills.Remove(b));

            //청구서 상태 확인. 확정 상태 청구서는 삭제할 수 없다.
            var billStatusCode = config.GetSection(nameof(BillStatusCode)).Get<BillStatusCode>() ?? new BillStatusCode();
            var certainedBills = bills.Where(b => b.StatusCode == billStatusCode.Certain).ToList();
            foreach (var centainedBill in certainedBills)
            {
                bills.Remove(centainedBill);
            }
            if (certainedBills.Any())
                response.AddFail(StatusCodes.Status400BadRequest, certainedBills.Select(b => b.BillId));

            ///청구서 엔티티에는 매입 매출의 구분이 없다. 같은 레코드도 조회 대상에 따라 매입/매출이 다를 수 있다.
            ///ex)acme가 발행한 고객용 청구서의 경우 - acme)매출 / 고객)매입
            ///청구서의 구매처(BuyerAccountId)와 로그인 유저의 accountId가 같으면 매입 청구서로 간주함.
            ///기존 user에 여러 account를 설정할 수 있을 경우엔 매입/매출 구분 기준으로 사용할 수 없는 문제
            ///를 해결하기 위해 user에는 단일 account만 할당할 수 있도록 변경 됨. 
            var purchaseBills = bills.Where(b => b.BuyerAccountId == accountUsers.First().AccountId).ToList();
            foreach (var purchaseBill in purchaseBills)
            {
                bills.Remove(purchaseBill);
            }
            response.AddFail(StatusCodes.Status422UnprocessableEntity, purchaseBills.Select(b => b.BillId));

            if (bills.Any())
                await billRepository.Delete(bills);

            var deleteBillIds = deletedBills.Select(b => b.BillId)
                                            .Union(bills.Select(b => b.BillId));
            response.Success.AddRange(deleteBillIds);
            return response;
        }

        private MultiHttpCodeResponse<BillResponse> CheckDulplicateBill(IEnumerable<Bill> bills)
        {
            var response = new MultiHttpCodeResponse<BillResponse>();

            var publishedBill = billRepository.GetLatestPublishedBill(
                bills.Select(b =>
                    ValueTuple.Create(b.BillDate,
                                      b.BuyerAccountId,
                                      b.ConsumptionAccountId)));

            var duplicatBills = bills.Where(b =>
                publishedBill.Select(pb =>
                    (pb.BillDate, pb.BuyerAccountId, pb.ConsumptionAccountId, pb.Amount))
                             .Any(s => b.BillDate == s.BillDate
                                       && b.ConsumptionAccountId == s.ConsumptionAccountId
                                       && b.BuyerAccountId == s.BuyerAccountId
                                       && b.Amount == s.Amount));

            if (duplicatBills.Any())
                response.AddFail(StatusCodes.Status409Conflict, duplicatBills.Select(db => db.OriginalBillId));
            return response;
        }

        private MultiHttpCodeResponse<BillResponse> CustomerNcpServices(List<BillItem> billItems)
        {
            var response = new MultiHttpCodeResponse<BillResponse>();

            var nullKeyIdBillItems = billItems
                .Where(bi => string.IsNullOrEmpty(bi.KeyId)
                            || string.IsNullOrWhiteSpace(bi.KeyId))
                .ToList();

            if (nullKeyIdBillItems.Any())
            {
                logger.LogWarning($"대상 청구서들의 bill item에 key id가 존재하지 않습니다. : " +
                    $"bill item id - ({string.Join(",", nullKeyIdBillItems.Select(bi => new { bi.BillId, bi.BillItemId }))})");

                nullKeyIdBillItems.ForEach(bi => billItems.Remove(bi));
                response.AddFail(StatusCodes.Status424FailedDependency,
                                 nullKeyIdBillItems.Select(bi => bi.Bill.OriginalBillId));

                if (billItems.Any() == false)
                    return response;
            }


            var masters = ncpRepository.GetLatestMasters(billItems.Select(bi => bi.KeyId)!)
                                       .ToHashSet();

            billItems = billItems.Join(masters,
                                       bi => bi.KeyId,
                                       m => m.KeyId,
                                       (bi, m) => { bi.NcpMaster = m; return bi; })
                                 .ToList();

            var billItemByAccount = billItems.GroupBy(bi => bi.NcpMaster?.Account);

            foreach (var billItemGroup in billItemByAccount)
            {
                switch (billItemGroup.Key)
                {
                    case "com":
                        var comResult = CheckComAccountBillItem(billItemGroup.ToHashSet());
                        response.AddFail(comResult.Fails);
                        break;
                    case "pcp":
                        var pcpResult = CheckPcpAccountBillItem(billItemGroup.ToHashSet());
                        response.AddFail(pcpResult.Fails);
                        break;
                    case "works":
                        var worksResult = CheckWorksAccountBillItem(billItemGroup.ToHashSet());
                        response.AddFail(worksResult.Fails);
                        break;
                    default:
                        break;
                }
            }
            return response;
        }

        private MultiHttpCodeResponse<BillResponse> CheckComAccountBillItem(IEnumerable<BillItem> billItems)
        {
            var response = new MultiHttpCodeResponse<BillResponse>();

            var configCode = config.GetSection(nameof(ContractChargeCode)).Get<ContractChargeCode>() ?? new ContractChargeCode();
            var productCode = config.GetSection(nameof(ProductCode)).Get<ProductCode>() ?? new ProductCode();
            var mspChargeProduct = _products?.FirstOrDefault(p => p.ProductCode == productCode?.NcpManagedService);
            var cutoffProduct = _products?.FirstOrDefault(p => p.ProductCode == productCode?.Cutoff100);

            if (mspChargeProduct == null)
            {
                logger.LogCritical($"네이버 클라우드 매니지드 서비스 상품을 찾을 수 없습니다. " +
                    $"Bill.dbo.Product 테이블 혹은 어플리케이션 구성 요소 ProductCode를 확인하세요" +
                    $"Configuration production code : {productCode?.NcpManagedService ?? "ProductCode 구성 요소 없음"}");

                response.AddFail(StatusCodes.Status424FailedDependency, billItems.Select(bi => bi.Bill.OriginalBillId));
                return response;
            }

            if (cutoffProduct == null)
            {
                logger.LogCritical($"네이버 클라우드 100원 미만 절사 상품을 찾을 수 없습니다. " +
                    $"Bill.dbo.Product 테이블 혹은 어플리케이션 구성 요소 ProductCode를 확인하세요" +
                    $"Configuration production code : {productCode?.Cutoff100 ?? "ProductCode 구성 요소 없음"}");
                response.AddFail(StatusCodes.Status424FailedDependency, billItems.Select(bi => bi.Bill.OriginalBillId));
                return response;
            }

            foreach (var billItem in billItems)
            {
                var mspConfig = billItem.Bill.ServiceHierarchyConfigs
                    .FirstOrDefault(s => s.ConfigCode == configCode?.MspCharge);

                if (mspConfig?.ConfigValue > 0)
                    AddMspCharge(mspChargeProduct, billItem, mspConfig);

                if (billItem.Bill.Amount % 100 > 0)
                    CutoffUnderHundred(cutoffProduct, billItem);
            }

            return response;
        }

        private MultiHttpCodeResponse<BillResponse> CheckPcpAccountBillItem(IEnumerable<BillItem> billItems)
        {
            var response = new MultiHttpCodeResponse<BillResponse>();

            var configCode = config.GetSection(nameof(ContractChargeCode)).Get<ContractChargeCode>();
            var productCode = config.GetSection(nameof(ProductCode)).Get<ProductCode>();
            var pcpDiscountProduct = _products?.FirstOrDefault(p => p.ProductCode == productCode?.NcpPcp);
            var mspChargeProduct = _products?.FirstOrDefault(p => p.ProductCode == productCode?.NcpManagedService);
            var cutoffProduct = _products?.FirstOrDefault(p => p.ProductCode == productCode?.Cutoff100);

            if (pcpDiscountProduct == null)
            {
                logger.LogCritical($"네이버 클라우드 PCP 약정 할인 상품을 찾을 수 없습니다. " +
                    $"Bill.dbo.Product 테이블 혹은 어플리케이션 구성 요소 ProductCode를 확인하세요" +
                    $"Configuration production code : {productCode?.NcpPcp ?? "ProductCode 구성 요소 없음"}");
                response.AddFail(StatusCodes.Status424FailedDependency, billItems.Select(bi => bi.Bill.OriginalBillId));
                return response;
            }

            if (mspChargeProduct == null)
            {
                logger.LogCritical($"네이버 클라우드 매니지드 서비스 상품을 찾을 수 없습니다. " +
                    $"Bill.dbo.Product 테이블 혹은 어플리케이션 구성 요소 ProductCode를 확인하세요" +
                    $"Configuration production code : {productCode?.NcpManagedService ?? "ProductCode 구성 요소 없음"}");
                response.AddFail(StatusCodes.Status424FailedDependency, billItems.Select(bi => bi.Bill.OriginalBillId));
                return response;
            }

            if (cutoffProduct == null)
            {
                logger.LogCritical($"네이버 클라우드 100원 미만 절사 상품을 찾을 수 없습니다. " +
                    $"Bill.dbo.Product 테이블 혹은 어플리케이션 구성 요소 ProductCode를 확인하세요" +
                    $"Configuration production code : {productCode?.Cutoff100 ?? "ProductCode 구성 요소 없음"}");
                response.AddFail(StatusCodes.Status424FailedDependency, billItems.Select(bi => bi.Bill.OriginalBillId));
                return response;
            }

            var keyIds = billItems.Where(bi => string.IsNullOrEmpty(bi.KeyId) == false || string.IsNullOrWhiteSpace(bi.KeyId) == false)
                                  .Select(bi => bi.KeyId)
                                  .Distinct();

            var marginProducts = ncpRepository.GetMarginExceptProducts();
            var ncpDetails = ncpRepository.GetLatestNcpDetails(keyIds!, marginProducts).ToHashSet();

            foreach (var billItem in billItems)
            {
                var pcpConfig = billItem.Bill.ServiceHierarchyConfigs
                    .FirstOrDefault(s => s.ConfigCode == configCode?.NcpPcpDiscount);

                if (pcpConfig == null || pcpConfig?.ConfigValue <= 0)
                    continue;

                if (pcpConfig == null)
                {
                    logger.LogInformation($"대상 고객에 대한 ServiceHierarchyConfig 레코드가 없습니다. " +
                        $"할인 금액을 계산하지 않습니다. account id - {billItem.Bill.ConsumptionAccountId} 원본 bill id - {billItem.Bill.OriginalBillId}");
                    continue;
                }

                var bill = billItem.Bill;
                var pcpDiscountBillItem = mapper.Map<BillItem>(billItem);
                pcpDiscountBillItem.Product = pcpDiscountProduct;
                pcpDiscountBillItem.Description = pcpDiscountProduct.ProductName;
                pcpDiscountBillItem.Amount =
                    -(ncpDetails.Where(nd => nd.KeyId == billItem.KeyId)
                              .Sum(nd => nd.UseAmount ?? decimal.Zero)
                    * ((decimal)pcpConfig!.ConfigValue / 100));

                bill.BillItems.Add(pcpDiscountBillItem);

                bill.DiscountAmount += pcpDiscountBillItem.Amount;
                bill.Amount = billItem.NcpMaster!.TotalDemandAmount!.Value
                    + pcpDiscountBillItem.Amount;
                bill.Tax = bill.Amount * (decimal)0.1;

                var mspChargeConfig = billItem.Bill.ServiceHierarchyConfigs
                    .FirstOrDefault(s => s.ConfigCode == configCode?.MspCharge);

                if (mspChargeConfig?.ConfigValue > 0)
                    AddMspCharge(mspChargeProduct, billItem, mspChargeConfig!);

                if (bill.Amount % 100 > 0)
                    CutoffUnderHundred(cutoffProduct, billItem);
            }

            return response;
        }

        private MultiHttpCodeResponse<BillResponse> CheckWorksAccountBillItem(IEnumerable<BillItem> billItems)
        {
            var response = new MultiHttpCodeResponse<BillResponse>();

            var configCode = config.GetSection(nameof(ContractChargeCode)).Get<ContractChargeCode>() ?? new ContractChargeCode();
            var productCode = config.GetSection(nameof(ProductCode)).Get<ProductCode>() ?? new ProductCode();
            var worksDiscountProduct = _products?.FirstOrDefault(p => p.ProductCode == productCode?.NcpWorks);
            var mspChargeProduct = _products?.FirstOrDefault(p => p.ProductCode == productCode?.NcpManagedService);
            var cutoffProduct = _products?.FirstOrDefault(p => p.ProductCode == productCode?.Cutoff100);

            if (worksDiscountProduct == null)
            {
                logger.LogCritical($"네이버 클라우드 works 약정 할인 상품을 찾을 수 없습니다. " +
                    $"Bill.dbo.Product 테이블 혹은 어플리케이션 구성 요소 ProductCode를 확인하세요" +
                    $"Configuration production code : {productCode?.NcpWorks ?? "ProductCode 구성 요소 없음"}");
                response.AddFail(StatusCodes.Status424FailedDependency, billItems.Select(bi => bi.Bill.OriginalBillId));
                return response;
            }

            if (mspChargeProduct == null)
            {
                logger.LogCritical($"네이버 클라우드 매니지드 서비스 상품을 찾을 수 없습니다. " +
                    $"Bill.dbo.Product 테이블 혹은 어플리케이션 구성 요소 ProductCode를 확인하세요" +
                    $"Configuration production code : {productCode?.NcpManagedService ?? "ProductCode 구성 요소 없음"}");
                response.AddFail(StatusCodes.Status424FailedDependency, billItems.Select(bi => bi.Bill.OriginalBillId));
                return response;
            }

            if (cutoffProduct == null)
            {
                logger.LogCritical($"네이버 클라우드 100원 미만 절사 상품을 찾을 수 없습니다. " +
                    $"Bill.dbo.Product 테이블 혹은 어플리케이션 구성 요소 ProductCode를 확인하세요" +
                    $"Configuration production code : {productCode?.Cutoff100 ?? "ProductCode 구성 요소 없음"}");
                response.AddFail(StatusCodes.Status424FailedDependency, billItems.Select(bi => bi.Bill.OriginalBillId));
                return response;
            }

            foreach (var billItem in billItems)
            {
                var worksConfig = billItem.Bill.ServiceHierarchyConfigs
                    .FirstOrDefault(s => s.ConfigCode == configCode?.NcpWorkDiscount);

                if (worksConfig?.ConfigValue <= 0)
                    continue;

                if (worksConfig == null)
                {
                    logger.LogInformation($"대상 고객에 대한 ServiceHierarchyConfig 레코드가 없습니다. " +
                        $"할인 금액을 계산하지 않습니다. account id - {billItem.Bill.ConsumptionAccountId} 원본 bill id - {billItem.Bill.OriginalBillId}");
                    continue;
                }

                var bill = billItem.Bill;
                var worksDiscountBillItem = mapper.Map<BillItem>(billItem);
                worksDiscountBillItem.Product = worksDiscountProduct;
                worksDiscountBillItem.Description = worksDiscountProduct.ProductName;
                worksDiscountBillItem.Amount = -(billItem.NcpMaster!.ProductDiscountAmount!.Value * ((decimal)worksConfig!.ConfigValue / 100));

                bill.BillItems.Add(worksDiscountBillItem);

                bill.DiscountAmount += worksDiscountBillItem.Amount;
                bill.Amount = billItem.NcpMaster!.DefaultAmount!.Value + worksDiscountBillItem.Amount;
                bill.Tax = bill.Amount * (decimal)0.1;

                var mspChargeConfig = billItem.Bill.ServiceHierarchyConfigs
                  .FirstOrDefault(s => s.ConfigCode == configCode?.MspCharge);

                if (mspChargeConfig?.ConfigValue > 0)
                    AddMspCharge(mspChargeProduct, billItem, mspChargeConfig!);

                if (bill.Amount % 100 > 0)
                    CutoffUnderHundred(cutoffProduct, billItem);
            }

            return response;
        }

        private void AddMspCharge(Product mspChargeProduct, BillItem billItem, ServiceHierarchyConfig mspChargeConfig)
        {
            var bill = billItem.Bill;

            var mspChargeBillItem = mapper.Map<BillItem>(billItem);
            mspChargeBillItem.Product = mspChargeProduct;
            mspChargeBillItem.Description = mspChargeProduct.ProductName;
            mspChargeBillItem.Amount = bill.Amount * ((decimal)mspChargeConfig!.ConfigValue / 100);
            bill.BillItems.Add(mspChargeBillItem);

            bill.ExtraAmount += mspChargeBillItem.Amount;
            bill.Amount += mspChargeBillItem.Amount;
            bill.Tax = bill.Amount * (decimal)0.1;
        }

        private void CutoffUnderHundred(Product CutoffProduct, BillItem billItem)
        {
            var cutoff = billItem.Bill.Amount % 100;
            var cutoffBillItem = mapper.Map<BillItem>(billItem);
            cutoffBillItem.Product = CutoffProduct;
            cutoffBillItem.Description = CutoffProduct.ProductName;
            cutoffBillItem.Amount = -cutoff;
            cutoffBillItem.KeyId = null;
            cutoffBillItem.VendorCode = null;
            billItem.Bill.BillItems.Add(cutoffBillItem);

            billItem.Bill.DiscountAmount += cutoffBillItem.Amount;
            billItem.Bill.Amount += cutoffBillItem.Amount;
            billItem.Bill.Tax = billItem.Bill.Amount * (decimal)0.1;
        }
    }

    internal record BillData
    {
        public Bill Bill { get; set; } = default!;
        public IEnumerable<ServiceHierarchyConfig> ServiceHierarchyConfigs { get; set; } = new List<ServiceHierarchyConfig>();
    }
}

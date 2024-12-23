using Billing.Data.Interfaces;
using BillingApiTres.Converters;
using BillingApiTres.Extensions;
using BillingApiTres.Models.Clients;
using BillingApiTres.Models.Dto;
using BillingApiTres.Models.Dto.Users;
using BillingApiTres.Models.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace BillingApiTres.Controllers.AccountUsers
{

    [Route("[controller]")]
    [Authorize]
    public class AccountUserController(AcmeGwClient gwClient,
                                       IBillRoleRepository billRoleRepository,
                                       IConfiguration config) : ControllerBase
    {
        /// <summary>
        /// 특정 account에 소속된 user 목록을 조회합니다.
        /// </summary>
        /// <param name="accountId">대상 account의 id</param>
        /// <returns></returns>
        [AuthorizeAccountIdFilter([nameof(accountId)])]
        [HttpGet("/accountUsers")]
        public async Task<ActionResult<List<AccountUserResponse>>> GetList([FromQuery][Required] long accountId)
        {
            var response = await gwClient.GetSerializedString(
                $"sales/accountUser?limit=999999&accountIdCsv={accountId}",
                HttpMethod.Get);

            var users = JsonSerializer.Deserialize<List<AccountUserResponse>>(response);

            if (users == null || users.Any() == false)
                return Ok();

            users = users.Where(u => u.UserInfo != null).ToList();

            var userRoleRes = await gwClient.GetSerializedString(
                $"/iam/authority/users/roles",
                HttpMethod.Post,
                JsonContent.Create(new { userId = users.Select(u => u.UserId) }));

            var userRoles = JsonSerializer.Deserialize<List<UserRoleResponse>>(userRoleRes) ?? new();
            var billRoles = billRoleRepository.GetAll();
            users.ForEach(
                u => u.Roles = userRoles.FirstOrDefault(ur => ur.UserId == u.UserId)?.Roles
                                        .Where(r => billRoles.Select(br => br.RoleId).Contains(r.RoleId))
                                        .Select(q => new BillRoleResponse
                                        {
                                            BillRoleId = q.RoleId,
                                            RoleName = q.RoleName
                                        }) ?? Enumerable.Empty<BillRoleResponse>());

            return users;
        }

        /// <summary>
        /// 대상 user를 account에서 제거합니다
        /// </summary>
        /// <param name="id">account user seq</param>
        [HttpDelete("/accountUsers/{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);

            var accountUser = await gwClient.Get<AccountUser>($"sales/accountUser/{id}", token?.RawData!);
            if (accountUser == null)
                return NoContent();

            if (HttpContext.AuthenticateAccountId([accountUser.AccountId]) == false)
                return Forbid();

            await gwClient.GetSerializedString($"sales/accountUser/{id}", HttpMethod.Delete);
            return NoContent();
        }
    }
}

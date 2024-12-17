using BillingApiTres.Converters;
using BillingApiTres.Extensions;
using BillingApiTres.Models.Clients;
using BillingApiTres.Models.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BillingApiTres.Controllers.AccountUsers
{

    [Route("[controller]")]
    [Authorize]
    public class AccountUserController(AcmeGwClient gwClient,
                                       IConfiguration config) : ControllerBase
    {
        /// <summary>
        /// 특정 account에 소속된 user 목록을 조회합니다.
        /// </summary>
        /// <param name="accountId">대상 account의 id</param>
        /// <returns></returns>
        [AuthorizeAccountIdFilter([nameof(accountId)])]
        [HttpGet("/accountUsers")]
        public async Task<string> GetList([FromQuery][Required] long accountId)
        {
            var response = await gwClient.GetSerializedString(
                $"sales/accountUser?limit=999999&accountIdCsv={accountId}",
                HttpMethod.Get);
            return response;
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

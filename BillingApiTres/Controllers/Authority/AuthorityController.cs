using BillingApiTres.Converters;
using BillingApiTres.Models.Clients;
using BillingApiTres.Models.Dto.Authority;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using BillingApiTres.Extensions;

namespace BillingApiTres.Controllers.Authority
{
    [Route("[controller]")]
    [Authorize]
    public class AuthorityController(AcmeGwClient gwClient,
                                     IConfiguration config,
                                     ILogger<AuthorityController> logger) : ControllerBase
    {
        /// <summary>
        /// 대상 유저에 특정 권한을 추가합니다
        /// </summary>
        /// <param name="userId">대상 유저 id</param>
        /// <param name="tenantId">tenant id</param>
        /// <param name="roleId">추가 권한 id</param>
        /// <returns></returns>
        [HttpPost("/authority/user/{userId}/roles")]
        public async Task<ActionResult<string>> Add(string userId,
                                            [FromQuery][Required] string tenantId,
                                            [FromBody] AuthorityPostRequest roleId)
        {
            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);

            var accountUsers = await gwClient.Get<List<AccountUser>>($"sales/accountUser?limit=99999&userUuid={userId}", token?.RawData!);
            if (accountUsers == null || accountUsers.Any() == false)
            {
                logger.LogWarning($"존재하지 않는 사용자 요청 : user id - {userId}");
                return BadRequest();
            }

            if (HttpContext.AuthenticateAccountId(accountUsers.Select(au => au.AccountId)) == false)
                return Forbid();

            var response = await gwClient.GetSerializedString(
                $"iam/authority/user/{userId}/roles?tenantId={tenantId}",
                HttpMethod.Post,
                JsonContent.Create(roleId));

            return Ok(response);
        }

        /// <summary>
        /// 대상 유저에서 특정 권한을 제거합니다
        /// </summary>
        /// <param name="userId">대상 유저 id</param>
        /// <param name="roleId">제거하는 권한 id</param>
        /// <returns></returns>
        [HttpDelete("/authority/user/{userId}/roles/{roleId}")]
        public async Task<ActionResult> Delete(string userId, string roleId)
        {
            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);

            var accountUsers = await gwClient.Get<List<AccountUser>>($"sales/accountUser?limit=99999&userUuid={userId}", token?.RawData!);
            if (accountUsers == null)
            {
                logger.LogWarning($"존재하지 않는 사용자 요청 : user id - {userId}");
                return BadRequest();
            }

            if (HttpContext.AuthenticateAccountId(accountUsers.Select(au => au.AccountId)) ==false)
                return Forbid();

            var response = await gwClient.GetSerializedString(
                $"iam/authority/user/{userId}/roles/{roleId}",
                HttpMethod.Delete);

            return NoContent();
        }
    }
}

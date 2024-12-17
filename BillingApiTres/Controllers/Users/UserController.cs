using Azure.Core;
using BillingApiTres.Converters;
using BillingApiTres.Models.Clients;
using BillingApiTres.Models.Dto.Users;
using BillingApiTres.Models.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using BillingApiTres.Extensions;

namespace BillingApiTres.Controllers.Users
{
    [Route("[controller]")]
    [Authorize]
    public class UserController(AcmeGwClient gwClient, ILogger<UserController> logger) : ControllerBase
    {
        /// <summary>
        /// 사용자 초대
        /// </summary>
        [HttpPost("/user-invite")]
        [AuthorizeAccountIdFilter([nameof(UserInviteRequest.AccountId)])]
        public async Task<ActionResult> Invite([FromBody] UserInviteRequest request)
        {
            var response = await gwClient.GetSerializedString("iam/user-invite",
                                                              HttpMethod.Post,
                                                              JsonContent.Create(request));
            return Ok();
        }

        /// <summary>
        /// 사용자의 비밀번호를 변경하는 이메일을 요청합니다
        /// </summary>
        /// <param name="userId">user id</param>
        [HttpPost("/user/{userId}/forgot-password")]
        public async Task<ActionResult> ChangePassword(string userId)
        {
            var token = JwtConverter.ExtractJwtToken(HttpContext.Request);
            var accountUsers = await gwClient.Get<List<AccountUser>>($"sales/accountUser?limit=99999&userUuid={userId}", token?.RawData!);

            if (HttpContext.AuthenticateAccountId(accountUsers.Select(u => u.AccountId)) == false)
                return Forbid();

            var response = await gwClient.GetSerializedString($"iam/user/{userId}/forgot-password",
                                                              HttpMethod.Post);
            return Ok();
        }
    }
}



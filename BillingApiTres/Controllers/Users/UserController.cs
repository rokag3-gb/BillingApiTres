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
        [HttpPost("/user-invite")]
        [AuthorizeAccountIdFilter([nameof(UserInviteRequest.AccountId)])]
        public async Task<ActionResult> Invite([FromBody] UserInviteRequest request)
        {
            var response = await gwClient.GetSerializedString("iam/user-invite",
                                                              HttpMethod.Post,
                                                              JsonContent.Create(request));
            return Ok();
        }

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



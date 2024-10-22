using System.IdentityModel.Tokens.Jwt;

namespace BillingApiTres.Converters
{
    public class JwtConverter
    {
        public static JwtSecurityToken? ExtractJwtToken(HttpRequest request)
        {
            var authorizationHeader = request.Headers["Authorization"].FirstOrDefault();
            if (authorizationHeader != null && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return new JwtSecurityToken(
                    authorizationHeader.Substring("Bearer ".Length).Trim());
            }
            return null;
        }
    }
}

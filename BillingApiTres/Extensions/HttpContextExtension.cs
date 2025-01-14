using System.Collections.Immutable;

namespace BillingApiTres.Extensions
{
    public static class HttpContextExtension
    {
        public static bool AuthenticateAccountId(this HttpContext context, IEnumerable<long> requestAccountIds)
        {
            var result = false;
            var grantedAccountIds = context.Items["X-Account"] as ImmutableHashSet<long>;

            if (grantedAccountIds?.Any(id => requestAccountIds.Contains(id)) == true)
                result = true;

            return result;
        }
    }
}

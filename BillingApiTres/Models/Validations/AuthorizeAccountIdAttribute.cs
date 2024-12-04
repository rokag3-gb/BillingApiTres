using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Immutable;

namespace BillingApiTres.Models.Validations
{
    /// <summary>
    /// 메서드에 특성으로 선언 가능한 ActionFilter를 정의합니다.
    /// 이름으로 지정된 인자의 값과 X-Account 헤더에 설정된 account id 값을 비교하여 유효한 요청인지 검사합니다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AuthorizeAccountIdFilter : ActionFilterAttribute
    {
        private readonly string[] _propertyNames = [];

        public AuthorizeAccountIdFilter(string[] propertyNames)
        {
            _propertyNames = propertyNames;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            if (context.HttpContext.Items.TryGetValue(config["AccountHeader"]!, out object? idValues) == false)
                throw new BadHttpRequestException("필요한 헤더가 구성되지 않았습니다. GW 관리자에게 문의하세요");

            var idHashSet = idValues as ImmutableHashSet<long>;
            if (idHashSet == null)
                throw new BadHttpRequestException("필요한 헤더가 구성되지 않았습니다. GW 관리자에게 문의하세요");

            foreach (var argument in context.ActionArguments)
            {
                if (argument.Value == null) continue;

                var properties = argument.Value.GetType()
                                               .GetProperties()
                                               .Where(p => _propertyNames.Contains(p.Name));
                if (properties?.Any() == true)
                {
                    foreach (var property in properties!)
                    {
                        var obj = property.GetValue(argument.Value);

                        if (ValidateId(idHashSet, obj) == false)
                        {
                            context.Result = new ForbidResult();
                            return;
                        }
                    }
                }
                else
                {
                    if (_propertyNames.Contains(argument.Key) == false)
                        continue;

                    if (ValidateId(idHashSet, argument.Value) == false)
                    {
                        context.Result = new ForbidResult();
                        return;
                    }
                }
            }
        }

        private bool ValidateId(ImmutableHashSet<long> idHashSet, object? propertyObject)
        {
            if (propertyObject is long idValue)
            {
                if (idHashSet.Contains(idValue))
                    return true;
            }
            else if (propertyObject is IEnumerable<long> ids)
            {
                if (ids.Any() && ids.All(idHashSet.Contains))
                    return true;
            }
            else if (propertyObject is string idString)
            {
                List<long> list = new();
                if (long.TryParse(idString, out long id))
                    list.Add(id);

                list.AddRange(
                    idString.Split(",", StringSplitOptions.TrimEntries)
                            .Select(id => { long.TryParse(id, out long i); return i; }));

                if (list.Any() && list.All(idHashSet.Contains))
                    return true;
            }

            return false;
        }
    }
}

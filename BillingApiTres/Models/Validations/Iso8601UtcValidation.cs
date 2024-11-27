using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace BillingApiTres.Models.Validations
{
    public class Iso8601UtcValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is not DateTime date)
                return new ValidationResult("날짜 형식이 아닙니다.", [validationContext.MemberName!]);

            if (IsValidIso8601(date))
                return ValidationResult.Success;

            return new ValidationResult("ISO 8601 UTC 가 아닙니다.", [validationContext.MemberName!]);
        }

        private bool IsValidIso8601(DateTime? input)
        {
            if (input == null)
                return false;

            return input.Value.Kind == DateTimeKind.Utc;
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace BillingApiTres.Models.Validations
{
    public class ServiceHierarchyConfigCodeValidation : ValidationAttribute
    {
        string[] validCodes = { "MspChargeRate", "WorksContractDiscountRate", "PcpContractDiscountRate" };

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is not string)
                return new ValidationResult("문자열 형식만 가능합니다.", [validationContext.MemberName!]);

            if (validCodes.Contains(value))
                return ValidationResult.Success;
            else
                return new ValidationResult($"{string.Join(", ", validCodes)}만 사용 가능합니다. 입력값 : {value}", [validationContext.MemberName!]);
        }
    }
}

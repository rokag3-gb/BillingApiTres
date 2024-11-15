using System.ComponentModel.DataAnnotations;

namespace BillingApiTres.Models.Validations
{
    public class ServiceHierarchyConfigCodeValidation : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is not string)
                return new ValidationResult("문자열 형식만 가능합니다.", [validationContext.MemberName!]);

            if (value.ToString() == "MspChargeRate" || value.ToString() == "WorksContractDiscountRate" || value.ToString() == "PcpContractDiscountRate")
                return ValidationResult.Success;
            else
                return new ValidationResult("사용할 수 없는 값입니다.", [validationContext.MemberName!]);
        }
    }
}

namespace Lumora.DataAnnotations
{
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string comparisonProperty;

        public DateGreaterThanAttribute(string comparisonProperty)
        {
            this.comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Ensure the current value is a valid DateTimeOffset?
            if (value is not DateTimeOffset currentValue)
            {
                return ValidationResult.Success; // Skip validation if the current value is null
            }

            // Get the comparison property info
            var property = validationContext.ObjectType.GetProperty(comparisonProperty);
            if (property == null)
            {
                throw new ArgumentException($"Property '{comparisonProperty}' not found.");
            }

            // Get the value of the comparison property
            var comparisonValue = property.GetValue(validationContext.ObjectInstance) as DateTimeOffset?;

            // Perform the comparison
            if (comparisonValue != null && currentValue <= comparisonValue)
            {
                var errorMessage = ErrorMessage ?? $"{validationContext.MemberName} must be greater than {comparisonProperty}.";
                return new ValidationResult(errorMessage);
            }

            return ValidationResult.Success;
        }
    }
}

using System.Text.RegularExpressions;

namespace Lumora.DataAnnotations
{
    public class CustomEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not string email)
            {
                return new ValidationResult("Invalid email address format.");
            }

            email = email.Trim().ToLower();

            var emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(email, emailRegex))
            {
                return new ValidationResult("Invalid email address format.");
            }

            return ValidationResult.Success;
        }
    }
}

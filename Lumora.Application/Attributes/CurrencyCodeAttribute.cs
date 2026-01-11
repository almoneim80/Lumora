using System.Diagnostics;

namespace Lumora.Application.Attributes
{
    public class CurrencyCodeAttribute : ValidationAttribute
    {
        public static string GetAllCurrencyCodesRegex()
        {
            var sb = new StringBuilder("^(");

            CultureInfo.GetCultures(CultureTypes.AllCultures).Where(c => !c.IsNeutralCulture).ToList().ForEach(c =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(c.Name))
                    {
                        var ri = new RegionInfo(c.Name);
                        if (ri.CurrencyNativeName.Length > 0)
                        {
                            sb.Append(ri.ISOCurrencySymbol);
                        }

                        sb.Append("|");
                    }
                }
                catch
                {
                    Debug.WriteLine("Cannot get ISOCurrencySymbol for culture. {CultureName}: ", c.Name);
                }
            });

            sb.Remove(sb.Length - 1, 1);
            sb.Append(")$");

            return sb.ToString();
        }

        protected override ValidationResult IsValid(object? value, System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            var currencyCode = value as string;

            if (currencyCode != null)
            {
                var symbol = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(c => !c.IsNeutralCulture).Select(culture =>
                {
                    try
                    {
                        return new RegionInfo(culture.Name);
                    }
                    catch
                    {
                        return null;
                    }
                }).Where(ri => ri != null && ri.ISOCurrencySymbol == currencyCode.ToUpper()).Select(ri => ri!.CurrencySymbol).FirstOrDefault();

                if (symbol != null)
                {
                    return ValidationResult.Success!;
                }
                else
                {
                    return new ValidationResult("Invalid Currency Code");
                }
            }
            else
            {
                return ValidationResult.Success!;
            }
        }
    }
}

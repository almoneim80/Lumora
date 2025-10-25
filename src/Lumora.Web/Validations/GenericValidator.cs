using System.Text.RegularExpressions;
using FluentValidation;

namespace Lumora.Validations;

public static class GenericValidator
{
    /// <summary>
    /// Validates that the value is not null, empty, or the default value.
    /// </summary>
    public static IRuleBuilderOptions<T, TProperty> MustNotBeDefault<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder)
    {
        return (IRuleBuilderOptions<T, TProperty>)ruleBuilder.Custom((value, context) =>
        {
            if (EqualityComparer<TProperty>.Default.Equals(value, default) ||
                (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                context.AddFailure($"The field '{context.PropertyName}' cannot be null, empty, contain only spaces, or be the default value (e.g., 0 for numbers).");
            }
        });
    }

    /// <summary>
    /// Check that the text contains only characters and spaces.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> MustContainOnlyLettersAndSpaces<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Matches(@"^[\p{L}\s]+$").WithMessage("This field can only contain letters and spaces.");
    }

    /// <summary>
    /// Check that the text matches the minimum and maximum length.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> MustHaveLengthInRange<T>(
        this IRuleBuilder<T, string?> ruleBuilder, int minLength, int maxLength)
    {
        return ruleBuilder
            .MaximumLength(maxLength).WithMessage($"This field cannot exceed {maxLength} characters.")
            .MinimumLength(minLength).WithMessage($"This field cannot be less than {minLength} characters.");
    }

    public static IRuleBuilderOptions<T, string?>? MustBeValidPhoneNumber<T>(
    this IRuleBuilder<T, string?> ruleBuilder,
    int minLength,
    int maxLength)
    {
        return ruleBuilder
            .Custom((phoneNumber, context) =>
            {
                if (!string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber != "")
                {
                    string sanitizedPhoneNumber = phoneNumber.Replace(" ", "");

                    if (!Regex.IsMatch(sanitizedPhoneNumber, @"^\+\d{1,3}\d{6,12}$"))
                        context.AddFailure("PhoneNumber", "Invalid phone number format. The number must start with a '+' followed by the country code and 6 to 12 digits.");

                    if (sanitizedPhoneNumber.Length < minLength)
                        context.AddFailure("PhoneNumber", $"Phone number must be at least {minLength} characters long.");

                    if (sanitizedPhoneNumber.Length > maxLength)
                        context.AddFailure("PhoneNumber", $"Phone number cannot exceed {maxLength} characters.");

                    if (!Regex.IsMatch(sanitizedPhoneNumber, @"^[\d\+]*$"))
                        context.AddFailure("PhoneNumber", "Phone number can only contain digits and an optional '+' sign at the beginning.");
                }
            }) as IRuleBuilderOptions<T, string?>;
    }

    /// <summary>
    /// Validates that a numeric value falls within a specified range.
    /// </summary>
    public static IRuleBuilderOptions<T, TProperty?> MustBeInRange<T, TProperty>(
        this IRuleBuilder<T, TProperty?> ruleBuilder,
        TProperty minValue,
        TProperty maxValue)
        where TProperty : struct, IComparable<TProperty>
    {
        return ruleBuilder
            .Must(value => value.HasValue && value.Value.CompareTo(minValue) >= 0 && value.Value.CompareTo(maxValue) <= 0)
            .WithMessage($"Value must be between {minValue} and {maxValue}.");
    }

    /// <summary>
    /// Validates that compare between numeric value.
    /// </summary>
    public static IRuleBuilderOptions<T, TProperty?> MustBeLessThanOrEqualTo<T, TProperty>(
    this IRuleBuilder<T, TProperty?> ruleBuilder,
    Func<T, TProperty?> otherPropertyFunc)
    where TProperty : struct, IComparable<TProperty>
    {
        return ruleBuilder.Must((dto, value) =>
        {
            var otherValue = otherPropertyFunc(dto);
            if (!value.HasValue || !otherValue.HasValue)
                return true; // Allow if either value is null (nullable handling)

            return value.Value.CompareTo(otherValue.Value) <= 0;
        }).WithMessage((dto, value) =>
        {
            var otherValue = otherPropertyFunc(dto);
            return $"The value {value} must be less than or equal to {otherValue} property.";
        });
    }

    /// <summary>
    /// Validates Time Span for duration is valid.
    /// </summary>
    public static IRuleBuilderOptions<T, TimeSpan?> MustBeWithinTimeRange<T>(
    this IRuleBuilder<T, TimeSpan?> ruleBuilder, TimeSpan? min, TimeSpan? max)
    {
        return ruleBuilder
            .Must(duration =>
                (!min.HasValue || duration >= min) && (!max.HasValue || duration <= max))
            .WithMessage($"Duration must be between {min} and {max}.");
    }

    /// <summary>
    /// Validates that a file path is valid, non-empty, and has an allowed extension.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> MustBeValidAttachment<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(path =>
            {
                if (!string.IsNullOrWhiteSpace(path) && path != "")
                {
                    return Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute);
                }
                return true;
            }).WithMessage("The file path is not valid.");
    }

    /// <summary>
    /// Validates that an image is valid and has an allowed extension.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> MustBeValidImage<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        var allowedImageExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".gif" };

        return ruleBuilder
            .Must(path =>
            {
                if (!string.IsNullOrWhiteSpace(path) && path != "")
                {
                    var extension = Path.GetExtension(path)?.ToLower() ?? string.Empty;
                    return allowedImageExtensions.Contains(extension);
                }
                return true;
            }).WithMessage($"Only image files ({string.Join(", ", allowedImageExtensions)}) are allowed.");
    }

    /// <summary>
    /// Validates that a vedio is valid and has an allowed extension.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> MustBeValidVideo<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        var allowedVideoExtensions = new List<string> { ".mp4", ".avi", ".mkv", ".mov" };
        return ruleBuilder
            .Must(path =>
            {
                if (!string.IsNullOrWhiteSpace(path) && path != "")
                {
                    var extension = Path.GetExtension(path)?.ToLower() ?? string.Empty;
                    return allowedVideoExtensions.Contains(extension);
                }
                return true;
            })
            .WithMessage($"Only video files ({string.Join(", ", allowedVideoExtensions)}) are allowed.");
    }

    /// <summary>
    /// Validates that a Document is valid and has an allowed extension.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> MustBeValidDocument<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        var allowedDocumentExtensions = new List<string> { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
        return ruleBuilder
            .Must(path =>
            {
                if (!string.IsNullOrWhiteSpace(path) && path != "")
                {
                    var extension = Path.GetExtension(path)?.ToLower() ?? string.Empty;
                    return allowedDocumentExtensions.Contains(extension);
                }
                return true;
            })
            .WithMessage($"Only document files ({string.Join(", ", allowedDocumentExtensions)}) are allowed.");
    }

    /// <summary>
    /// Validates a date against custom rules, such as required, range, past, or future.
    /// </summary>
    public static IRuleBuilderOptions<T, DateTimeOffset?> MustBeValidDate<T>(
    this IRuleBuilder<T, DateTimeOffset?> ruleBuilder,
    DateTimeOffset? minDate = null,
    DateTimeOffset? maxDate = null,
    bool mustBePast = false,
    bool mustBeFuture = false)
    {
        return ruleBuilder
            .Must(date =>
            {
                // If the value is null or contains only spaces, there is no issue
                if (!date.HasValue) return true;

                var utcNow = DateTimeOffset.UtcNow;

                // Check the date for the minimum
                if (minDate.HasValue && date.Value < minDate.Value) return false;

                // Check the date for the maximum
                if (maxDate.HasValue && date.Value > maxDate.Value) return false;

                // Check if the date is in the past or future
                if (mustBePast && date.Value >= utcNow) return false;
                if (mustBeFuture && date.Value <= utcNow) return false;

                return true;
            })
            .WithMessage(x =>
            {
                if (mustBePast)
                    return "Date must be in the past.";
                if (mustBeFuture)
                    return "Date must be in the future.";

                if (minDate.HasValue && maxDate.HasValue)
                    return $"Date must be between {minDate.Value.ToString("d")} and {maxDate.Value.ToString("d")}.";

                return "Invalid date. Ensure it meets the specified constraints.";
            });
    }

    /// <summary>
    /// Validates a Description against custom rules.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> MustBeValidDescription<T>(
    this IRuleBuilder<T, string?> ruleBuilder,
    int minLength = 5,
    int maxLength = 500,
    bool allowSpecialCharacters = false)
    {
        return ruleBuilder
            .Must(description =>
            {
                if (!string.IsNullOrWhiteSpace(description) && description != "")
                {
                    if (description.Length < minLength || description.Length > maxLength)
                    {
                        return false;
                    }

                    if (!allowSpecialCharacters && description.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
                    {
                        return false;
                    }

                    return true;
                }

                return true;
            }).WithMessage($"Description must be between {minLength} and {maxLength} characters and contain only allowed characters.");
    }

    /// <summary>
    /// Validates date with another date.
    /// </summary>
    public static IRuleBuilderOptions<T, TProperty?> MustBeEarlierThan<T, TProperty>(
        this IRuleBuilder<T, TProperty?> ruleBuilder,
        Func<T, TProperty?> otherPropertyFunc,
        Func<T, string> otherDateDisplayFunc)
        where TProperty : struct, IComparable<TProperty>
    {
        return ruleBuilder.Must((dto, value) =>
        {
            var otherValue = otherPropertyFunc(dto);
            if (!value.HasValue || !otherValue.HasValue)
                return true; // Allow if either value is null (nullable handling)

            return value.Value.CompareTo(otherValue.Value) < 0;
        })
        .WithMessage((dto, value) =>
            $"This date must be earlier than {otherDateDisplayFunc(dto)}.");
    }
}

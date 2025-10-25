namespace Lumora.Interfaces;
public interface ILocalizationManager
{
    /// <summary>
    /// Gets the cultures.
    /// </summary>
    IEnumerable<CultureDto> GetCultures();

    /// <summary>
    /// Initializes a new instance of the <see cref="ILocalizationManager"/> class.
    /// Gets the cultures.
    /// </summary>
    void SetCulture(string culture);

    /// <summary>
    /// Gets the countries.
    /// </summary>
    IEnumerable<CountryInfo> GetCountries();

    /// <summary>
    /// Gets the parental ratings.
    /// </summary>
    IEnumerable<ParentalRating> GetParentalRatings();

    /// <summary>
    /// Gets the rating level.
    /// </summary>
    int? GetRatingLevel(string rating, string? countryCode = null);

    /// <summary>
    /// Gets the localized string.
    /// </summary>
    string GetLocalizedString(string phrase, string culture);

    /// <summary>
    /// Gets the localized string.
    /// </summary>
    string GetLocalizedString(string phrase);

    /// <summary>
    /// Gets the localization options.
    /// </summary>
    IEnumerable<LocalizationOption> GetLocalizationOptions();

    /// <summary>
    /// Returns the correct <see cref="CultureDto" /> for the given language.
    /// </summary>
    CultureDto? FindLanguageInfo(string language);

    /// <summary>
    /// Gets the localized string with arguments replaced.
    /// </summary>
    string GetLocalizedStringWithReplaced(string key, params object[] args);
}

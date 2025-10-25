using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;

namespace Lumora.Services;

/// <summary>
/// Class LocalizationManager.
/// </summary>
public class LocalizationManager : ILocalizationManager
{
    private const string DefaultCulture = "ar";
    private const string RatingsPath = "Lumora.Localization.Ratings.";
    private const string CulturesPath = "Lumora.Localization.iso6392.txt";
    private const string CountriesPath = "Lumora.Localization.countries.json";
    private static readonly Assembly _assembly = typeof(LocalizationManager).Assembly;
    private static readonly string[] _unratedValues = { "n/a", "unrated", "not rated", "nr" };

    private readonly IServerConfigurationManager _configurationManager;
    private readonly ILogger<LocalizationManager> _logger;
    private readonly IConfiguration _configuration;

    private readonly Dictionary<string, Dictionary<string, ParentalRating>> _allParentalRatings =
        new Dictionary<string, Dictionary<string, ParentalRating>>(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _dictionaries =
        new ConcurrentDictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

    // private readonly JsonSerializerOptions _jsonOptions = JsonDefaults.Options;
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private List<CultureDto> _cultures = new List<CultureDto>();
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationManager" /> class.
    /// </summary>
    public LocalizationManager(
        IServerConfigurationManager configurationManager,
        IHttpContextAccessor httpContextAccessor,
        ILogger<LocalizationManager> logger,
        IConfiguration configuration)
    {
        _configurationManager = configurationManager;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    /// <summary>
    /// Loads all resources into memory.
    /// </summary>
    /// <returns><see cref="Task" />.</returns>
    public async Task LoadAll()
    {
        // Extract from the assembly
        foreach (var resource in _assembly.GetManifestResourceNames())
        {
            if (!resource.StartsWith(RatingsPath, StringComparison.Ordinal))
            {
                continue;
            }

            string countryCode = resource.Substring(RatingsPath.Length, 2);
            var dict = new Dictionary<string, ParentalRating>(StringComparer.OrdinalIgnoreCase);

            var stream = _assembly.GetManifestResourceStream(resource);
            await using (stream!.ConfigureAwait(false)) // shouldn't be null here, we just got the resource path from Assembly.GetManifestResourceNames()
            {
                using var reader = new StreamReader(stream!);
                string? line;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    string[] parts = line.Split(',');
                    if (parts.Length == 2
                        && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                    {
                        var name = parts[0];
                        dict.Add(name, new ParentalRating(name, value));
                    }
                    else
                    {
                        _logger.LogWarning("Malformed line in ratings file for country {CountryCode}", countryCode);
                    }
                }
            }

            _allParentalRatings[countryCode] = dict;
        }

        await LoadCultures().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the cultures.
    /// </summary>
    /// <returns><see cref="IEnumerable{CultureDto}" />.</returns>
    public IEnumerable<CultureDto> GetCultures()
        => _cultures;

    private async Task LoadCultures()
    {
        var assembly = typeof(LocalizationManager).Assembly;
        var resourceNames = assembly.GetManifestResourceNames();
        foreach (var name in resourceNames)
        {
            Console.WriteLine($"Available resource: {name}");
        }

        List<CultureDto> list = new List<CultureDto>();

        await using var stream = _assembly.GetManifestResourceStream(CulturesPath)
            ?? throw new InvalidOperationException($"Invalid resource path: '{CulturesPath}'");
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split('|');

            if (parts.Length == 5)
            {
                string name = parts[3];
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                string twoCharName = parts[2];
                if (string.IsNullOrWhiteSpace(twoCharName))
                {
                    continue;
                }

                string[] threeletterNames;
                if (string.IsNullOrWhiteSpace(parts[1]))
                {
                    threeletterNames = new[] { parts[0] };
                }
                else
                {
                    threeletterNames = new[] { parts[0], parts[1] };
                }

                list.Add(new CultureDto(name, name, twoCharName, threeletterNames));
            }
        }

        _cultures = list;
    }

    /// <inheritdoc />
    public CultureDto? FindLanguageInfo(string language)
    {
        // TODO language should ideally be a ReadOnlySpan but moq cannot mock ref structs
        for (var i = 0; i < _cultures.Count; i++)
        {
            var culture = _cultures[i];
            if (language.Equals(culture.DisplayName, StringComparison.OrdinalIgnoreCase)
                || language.Equals(culture.Name, StringComparison.OrdinalIgnoreCase)
                || Array.Exists<string>((string[])culture.ThreeLetterISOLanguageNames, x => x.Equals(language, StringComparison.OrdinalIgnoreCase))
                || language.Equals(culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
            {
                return culture;
            }
        }

        return default;
    }

    /// <inheritdoc />
    public IEnumerable<CountryInfo> GetCountries()
    {
        using StreamReader reader = new StreamReader(
            _assembly.GetManifestResourceStream(CountriesPath) ?? throw new InvalidOperationException($"Invalid resource path: '{CountriesPath}'"));
        return JsonSerializer.Deserialize<IEnumerable<CountryInfo>>(reader.ReadToEnd(), _jsonOptions)
            ?? throw new InvalidOperationException($"Resource contains invalid data: '{CountriesPath}'");
    }

    /// <inheritdoc />
    public IEnumerable<ParentalRating> GetParentalRatings()
    {
        // Use server default language for ratings
        // Fall back to empty list if there are no parental ratings for that language
        var ratings = GetParentalRatingsDictionary()?.Values.ToList()
            ?? new List<ParentalRating>();

        // Unrated
        if (!ratings.Exists(x => x.Value is null))
        {
            ratings.Add(new ParentalRating("Unrated", null));
        }

        // Minimum rating possible
        if (ratings.TrueForAll(x => x.Value != 0))
        {
            ratings.Add(new ParentalRating("Approved", 0));
        }

        // Matches PG (this has different age restrictions depending on country)
        if (ratings.TrueForAll(x => x.Value != 10))
        {
            ratings.Add(new ParentalRating("10", 10));
        }

        // Matches PG-13
        if (ratings.TrueForAll(x => x.Value != 13))
        {
            ratings.Add(new ParentalRating("13", 13));
        }

        // Matches TV-14
        if (ratings.TrueForAll(x => x.Value != 14))
        {
            ratings.Add(new ParentalRating("14", 14));
        }

        // Catchall if max rating of country is less than 21
        // Using 21 instead of 18 to be sure to allow access to all rated content except adult and banned
        if (!ratings.Exists(x => x.Value >= 21))
        {
            ratings.Add(new ParentalRating("21", 21));
        }

        // A lot of countries don't explicitly have a separate rating for adult content
        if (ratings.TrueForAll(x => x.Value != 1000))
        {
            ratings.Add(new ParentalRating("XXX", 1000));
        }

        // A lot of countries don't explicitly have a separate rating for banned content
        if (ratings.TrueForAll(x => x.Value != 1001))
        {
            ratings.Add(new ParentalRating("Banned", 1001));
        }

        return ratings.OrderBy(r => r.Value);
    }

    /// <summary>
    /// Gets the parental ratings dictionary.
    /// </summary>
    /// <param name="countryCode">The optional two letter ISO language string.</param>
    /// <returns><see cref="Dictionary{String, ParentalRating}" />.</returns>
    private Dictionary<string, ParentalRating>? GetParentalRatingsDictionary(string? countryCode = null)
    {
        // Fallback to server default if no country code is specified.
        if (string.IsNullOrEmpty(countryCode))
        {
            countryCode = _configurationManager.Configuration.MetadataCountryCode;
        }

        if (_allParentalRatings.TryGetValue(countryCode, out var countryValue))
        {
            return countryValue;
        }

        return null;
    }

    /// <inheritdoc />
    public int? GetRatingLevel(string rating, string? countryCode = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(rating);

        // Handle unrated content
        if (_unratedValues.Contains(rating, StringComparer.OrdinalIgnoreCase))
        {
            return null;
        }

        // Convert integers directly
        // This may override some of the locale specific age ratings (but those always map to the same age)
        if (int.TryParse(rating, out var ratingAge))
        {
            return ratingAge;
        }

        // Fairly common for some users to have "Rated R" in their rating field
        rating = rating.Replace("Rated :", string.Empty, StringComparison.OrdinalIgnoreCase);
        rating = rating.Replace("Rated ", string.Empty, StringComparison.OrdinalIgnoreCase);

        // Use rating system matching the language
        if (!string.IsNullOrEmpty(countryCode))
        {
            var ratingsDictionary = GetParentalRatingsDictionary(countryCode);
            if (ratingsDictionary is not null && ratingsDictionary.TryGetValue(rating, out ParentalRating? value))
            {
                return value.Value;
            }
        }
        else
        {
            // Fall back to server default language for ratings check
            // If it has no ratings, use the US ratings
            var ratingsDictionary = GetParentalRatingsDictionary() ?? GetParentalRatingsDictionary("us");
            if (ratingsDictionary is not null && ratingsDictionary.TryGetValue(rating, out ParentalRating? value))
            {
                return value.Value;
            }
        }

        // If we don't find anything, check all ratings systems
        foreach (var dictionary in _allParentalRatings.Values)
        {
            if (dictionary.TryGetValue(rating, out var value))
            {
                return value.Value;
            }
        }

        // Try splitting by : to handle "Germany: FSK-18"
        if (rating.Contains(':', StringComparison.OrdinalIgnoreCase))
        {
            int colonIndex = rating.LastIndexOf(':');
            if (colonIndex != -1 && colonIndex < rating.Length - 1)
            {
                string ratingLevelRightPart = rating.Substring(colonIndex + 1).Trim();
                if (!string.IsNullOrEmpty(ratingLevelRightPart))
                {
                    return GetRatingLevel(ratingLevelRightPart);
                }
            }
        }

        // Handle prefix country code to handle "DE-18"
        if (rating.Contains('-', StringComparison.OrdinalIgnoreCase))
        {
            int dashIndex = rating.IndexOf('-');
            if (dashIndex != -1)
            {
                // Extract culture from country prefix
                string leftPart = rating.Substring(0, dashIndex);
                var culture = FindLanguageInfo(leftPart);

                // Check if there's content after the dash
                if (dashIndex < rating.Length - 1)
                {
                    string rightPart = rating.Substring(dashIndex + 1);
                    if (!string.IsNullOrEmpty(rightPart))
                    {
                        // Check rating system of culture
                        return GetRatingLevel(rightPart, culture?.TwoLetterISOLanguageName);
                    }
                }
            }
        }

        return null;
    }

    /// <inheritdoc />
    public string GetLocalizedString(string phrase)
    {
        // return GetLocalizedString(phrase, _configurationManager.Configuration.UICulture);
        var culture = _httpContextAccessor.HttpContext?.Request.Cookies["PreferredCulture"]
                        ?? CultureInfo.CurrentCulture.Name;
        return GetLocalizedString(phrase, culture);
    }

    /// <inheritdoc />
    public string GetLocalizedString(string phrase, string culture)
    {
        if (string.IsNullOrEmpty(culture))
        {
            culture = _configurationManager.Configuration.UICulture;
        }

        if (string.IsNullOrEmpty(culture))
        {
            culture = DefaultCulture;
        }

        var dictionary = GetLocalizationDictionary(culture);

        if (dictionary.TryGetValue(phrase, out var value))
        {
            return value;
        }

        return phrase;
    }

    private Dictionary<string, string> GetLocalizationDictionary(string culture)
    {
        ArgumentException.ThrowIfNullOrEmpty(culture);

        const string Prefix = "Core";

        return _dictionaries.GetOrAdd(
            culture,
            static (key, localizationManager) => localizationManager.GetDictionary(Prefix, key, DefaultCulture + ".json").GetAwaiter().GetResult(),
            this);
    }

    private async Task<Dictionary<string, string>> GetDictionary(string prefix, string culture, string baseFilename)
    {
        ArgumentException.ThrowIfNullOrEmpty(culture);

        var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var namespaceName = GetType().Namespace + "." + prefix;

        await CopyInto(dictionary, namespaceName + "." + baseFilename).ConfigureAwait(false);
        await CopyInto(dictionary, namespaceName + "." + GetResourceFilename(culture)).ConfigureAwait(false);

        return dictionary;
    }

    private async Task CopyInto(IDictionary<string, string> dictionary, string resourcePath)
    {
        await using var stream = _assembly.GetManifestResourceStream(resourcePath);
        // If a Culture doesn't have a translation the stream will be null and it defaults to en-us further up the chain
        if (stream is null)
        {
            _logger.LogError("Missing translation/culture resource: {ResourcePath}", resourcePath);
            return;
        }

        var dict = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream, _jsonOptions).ConfigureAwait(false);
        if (dict is null)
        {
            throw new InvalidOperationException($"Resource contains invalid data: '{stream}'");
        }

        foreach (var key in dict.Keys)
        {
            dictionary[key] = dict[key];
        }
    }

    private static string GetResourceFilename(string culture)
    {
        var parts = culture.Split('-');

        if (parts.Length == 2)
        {
            culture = parts[0].ToLowerInvariant() + "-" + parts[1].ToUpperInvariant();
        }
        else
        {
            culture = culture.ToLowerInvariant();
        }

        return culture + ".json";
    }

    /// <inheritdoc />
    public IEnumerable<LocalizationOption> GetLocalizationOptions()
    {
        var options = _configuration.GetSection("LocalizationOptions").Get<List<LocalizationOption>>();
        return options ?? new List<LocalizationOption>();
    }

    // additional method
    public void SetCulture(string culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            throw new ArgumentException("Culture cannot be null or empty.", nameof(culture));
        }

        var cultureInfo = new CultureInfo(culture);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;

        // يمكنك إضافة منطق إضافي هنا، مثل تحديث التكوين أو إعادة تحميل الموارد إذا لزم الأمر
        _configurationManager.Configuration.UICulture = culture;
    }

    public string GetLocalizedStringWithReplaced(string key, params object[] args)
    {
        var phrase = GetLocalizedString(key);
        return args?.Length > 0 ? string.Format(phrase, args) : phrase;
    }
}

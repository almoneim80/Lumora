using Lumora.DTOs.StaticContent;

namespace Lumora.Interfaces.StaticContentIntf
{
    public interface IStaticContentService
    {
        /// <summary>
        /// Get a single content value by key and language.
        /// </summary>
        Task<string?> GetValueAsync(string key, string language = "ar");

        /// <summary>
        /// Get the full StaticContent entry by key and language.
        /// </summary>
        Task<StaticContent?> GetAsync(string key, string language = "ar");

        /// <summary>
        /// Get multiple contents by keys and language (for front-end).
        /// </summary>
        Task<List<StaticContent>> GetByKeysAsync(IEnumerable<string> keys, string language = "ar");

        /// <summary>
        /// Get all static content items optionally filtered by group, language or isActive.
        /// </summary>
        Task<List<StaticContent>> GetAllAsync(string? group = null, string? language = null, bool? isActive = true);

        /// <summary>
        /// Create or update a content entry (Upsert).
        /// </summary>
        Task<GeneralResult> SetValueAsync(string key, string value, string language = "ar");

        /// <summary>
        /// Create or update a full content object (used by Admin Panel).
        /// </summary>
        Task<GeneralResult> SaveAsync(StaticContentCreateDto content);

        /// <summary>
        /// Delete a content entry by key and language (Soft or Hard delete as per business rule).
        /// </summary>
        Task<GeneralResult> DeleteAsync(string key, string language = "ar");
    }
}

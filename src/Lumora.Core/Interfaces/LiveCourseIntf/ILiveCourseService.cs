using Lumora.DTOs.LiveCourse;

namespace Lumora.Interfaces.LiveCourseIntf
{
    public interface ILiveCourseService
    {
        /// <summary>
        /// Creates a new live course based on the provided data.
        /// Only accessible to authorized administrators.
        /// </summary>
        Task<GeneralResult<int>> CreateAsync(LiveCourseCreateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the details of an existing live course.
        /// Fails if the course does not exist or is deleted.
        /// </summary>
        Task<GeneralResult> UpdateAsync(int courseId, LiveCourseUpdateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the full details of a specific live course by its ID.
        /// Returns not found if the course does not exist.
        /// </summary>
        Task<GeneralResult<LiveCourseDetailsDto>> GetByIdAsync(int courseId, CancellationToken cancellationToken);

        /// <summary>
        /// Returns a list of all live courses, optionally filtered later by status or keyword.
        /// Only active courses should be visible to end users.
        /// </summary>
        Task<GeneralResult<PagedResult<LiveCourseListItemDto>>> GetListAsync(LiveCourseFilterDto filter, CancellationToken cancellationToken);

        /// <summary>
        /// Permanently deletes a live course from the system.
        /// This operation should ensure no users are currently enrolled.
        /// </summary>
        Task<GeneralResult> DeleteAsync(int courseId, CancellationToken cancellationToken);


        /// <summary>
        /// Changes the visibility status of a live course (active/inactive).
        /// Used to publish or unpublish a course from the frontend.
        /// </summary>
        Task<GeneralResult> SetActiveStatusAsync(int courseId, bool isActive, CancellationToken cancellationToken);

        /// <summary>
        /// Enrolls a user into a specific live course.
        /// Requires an optional reference to a successful PaymentItem.
        /// </summary>
        Task<GeneralResult> SubscribeUserAsync(int courseId, string userId, int? paymentItemId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the list of live courses that the user is currently enrolled in.
        /// Returns only active and valid enrollments.
        /// </summary>
        Task<GeneralResult<PagedResult<LiveCourseListItemDto>>> GetUserCoursesAsync(string userId, PaginationRequestDto pagination, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves all users who are currently enrolled in a specific live course.
        /// Typically used by administrators to manage participation.
        /// </summary>
        Task<GeneralResult<PagedResult<UserLiveCourseDto>>> GetCourseSubscribersAsync(int courseId, PaginationRequestDto pagination, CancellationToken cancellationToken);
    }
}

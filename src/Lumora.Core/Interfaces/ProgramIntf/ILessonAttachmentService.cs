namespace Lumora.Interfaces.ProgramIntf
{
    public interface ILessonAttachmentService
    {
        /// <summary>
        /// Add single attachment to lesson.
        /// </summary>
        Task<GeneralResult> AddSingleAttachmentAsync(SingleLessonAttachmentCreateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Update attachment data.
        /// </summary>
        Task<GeneralResult> UpdateAttachmentAsync(int attachmentId, LessonAttachmentUpdateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Delete single attachment.
        /// </summary>
        Task<GeneralResult> DeleteSingleAttachmentAsync(int attachmentId, CancellationToken cancellationToken);

        /// <summary>
        /// Get attachment by id.
        /// </summary>
        Task<GeneralResult<LessonAttachmentDetailsDto>> GetAttachmentByIdAsync(int attachmentId, CancellationToken cancellationToken);

        /// <summary>
        /// Get all attachments for specific lesson.
        /// </summary>
        Task<GeneralResult<List<LessonAttachmentDetailsDto>>> GetAttachmentsByLessonIdAsync(int lessonId, CancellationToken cancellationToken);

        /// <summary>
        /// Increment open count.
        /// </summary>
        Task<GeneralResult> IncrementOpenCountAsync(int attachmentId, CancellationToken cancellationToken);
    }
}

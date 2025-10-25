using Lumora.Interfaces.ProgramIntf;

namespace Lumora.Services.Programs
{
    public class LessonAttachmentService(
        PgDbContext dbContext, IMapper mapper, ILogger<LessonAttachmentService> logger, LessonAttachmentMessage messages) : ILessonAttachmentService
    {
        private readonly PgDbContext _dbContext = dbContext;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<LessonAttachmentService> _logger = logger;
        private readonly LessonAttachmentMessage _messages = messages;

        /// <inheritdoc/>
        public async Task<GeneralResult> AddSingleAttachmentAsync(SingleLessonAttachmentCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var lessonExists = await _dbContext.CourseLessons
                    .AnyAsync(l => l.Id == dto.LessonId && !l.IsDeleted, cancellationToken);
                if (!lessonExists)
                {
                    _logger.LogError("LessonAttachmentService - AddSingleAttachmentAsync : Lesson with ID {LessonId} not found or deleted.", dto.LessonId);
                    return new GeneralResult(false, _messages.MsgLessonNotFound, null, ErrorType.NotFound);
                }

                var attachment = _mapper.Map<LessonAttachment>(dto);

                await _dbContext.LessonAttachments.AddAsync(attachment, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("LessonAttachmentService - AddSingleAttachmentAsync : Attachment added successfully to lesson {LessonId}.", dto.LessonId);
                return new GeneralResult(true, _messages.MsgAttachmentAdded, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LessonAttachmentService - AddSingleAttachmentAsync : Error adding attachment to lesson {LessonId}.", dto.LessonId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("add single attachment"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> UpdateAttachmentAsync(int attachmentId, LessonAttachmentUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var attachment = await _dbContext.LessonAttachments
                    .FirstOrDefaultAsync(a => a.Id == attachmentId && !a.IsDeleted, cancellationToken);
                if (attachment == null)
                    return new GeneralResult(false, _messages.MsgAttachmentNotFound, null, ErrorType.NotFound);

                if (dto.LessonId != null)
                {
                    var lesson = await _dbContext.CourseLessons.AsNoTracking()
                        .FirstOrDefaultAsync(l => l.Id == dto.LessonId && !l.IsDeleted, cancellationToken);
                    if (lesson == null)
                    {
                        _logger.LogError("LessonAttachmentService - UpdateAttachmentAsync : Lesson with ID {LessonId} not found or deleted.", dto.LessonId);
                        return new GeneralResult(false, _messages.MsgLessonNotFound, null, ErrorType.NotFound);
                    }

                    attachment.LessonId = dto.LessonId.Value;
                }

                if (!string.IsNullOrWhiteSpace(dto.FileUrl))
                    attachment.FileUrl = dto.FileUrl;

                attachment.UpdatedAt = DateTimeOffset.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"LessonAttachmentService - UpdateAttachmentAsync : Attachment with Id {attachmentId} updated successfully.");
                return new GeneralResult(true, _messages.MsgAttachmentUpdated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LessonAttachmentService - UpdateAttachmentAsync : Error updating attachment.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("updating attachment"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> DeleteSingleAttachmentAsync(int attachmentId, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;

            try
            {
                var attachment = await _dbContext.LessonAttachments.FirstOrDefaultAsync(a => a.Id == attachmentId && !a.IsDeleted, cancellationToken);

                _logger.LogInformation($"LessonAttachmentService - DeleteSingleAttachmentAsync : Attempting to delete attachment with Id {attachmentId}");
                if (attachment == null) return new GeneralResult(false, _messages.MsgAttachmentNotFound, null, ErrorType.NotFound);

                attachment.IsDeleted = true;
                attachment.DeletedAt = now;
                attachment.UpdatedAt = now;

                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation($"LessonAttachmentService - DeleteSingleAttachmentAsync : Attachment with Id {attachmentId} deleted successfully.");
                return new GeneralResult(true, _messages.MsgAttachmentDeleted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LessonAttachmentService - DeleteSingleAttachmentAsync : Error deleting attachment");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("delete attachment."), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<LessonAttachmentDetailsDto>> GetAttachmentByIdAsync(int attachmentId, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                var attachment = await _dbContext.LessonAttachments
                    .AsNoTracking()
                    .Include(a => a.CourseLesson)
                    .FirstOrDefaultAsync(a => a.Id == attachmentId && !a.IsDeleted, cancellationToken);

                if (attachment == null)
                {
                    _logger.LogWarning("LessonAttachmentService - GetAttachmentByIdAsync : Attachment with Id {AttachmentId} not found.", attachmentId);
                    return new GeneralResult<LessonAttachmentDetailsDto>(
                        false, _messages.MsgAttachmentNotFound, null, ErrorType.NotFound);
                }

                var dto = new LessonAttachmentDetailsDto
                {
                    AttachmentId = attachment.Id,
                    LessonId = attachment.LessonId,
                    LessonName = attachment.CourseLesson?.Name ?? "N/A",
                    AttachmentUrl = attachment.FileUrl,
                    OpenCount = attachment.OpenCount,
                    CreatedAt = attachment.CreatedAt ?? now,
                    UpdatedAt = attachment.UpdatedAt ?? now
                };

                _logger.LogInformation("LessonAttachmentService - GetAttachmentByIdAsync : Attachment with Id {AttachmentId} retrieved successfully.", attachmentId);
                return new GeneralResult<LessonAttachmentDetailsDto>(
                    true, _messages.MsgAttachmentRetrieved, dto, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LessonAttachmentService - GetAttachmentByIdAsync : Error retrieving attachment with Id {AttachmentId}", attachmentId);
                return new GeneralResult<LessonAttachmentDetailsDto>(
                    false, _messages.GetUnexpectedErrorMessage("retrieving attachment"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<LessonAttachmentDetailsDto>>> GetAttachmentsByLessonIdAsync(int lessonId, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                var lesson = await _dbContext.CourseLessons
                    .AsNoTracking()
                    .FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted, cancellationToken);

                if (lesson == null)
                {
                    _logger.LogWarning("LessonAttachmentService - GetAttachmentsByLessonIdAsync : Lesson with Id {LessonId} not found.", lessonId);
                    return new GeneralResult<List<LessonAttachmentDetailsDto>>(
                        false, _messages.MsgLessonNotFound, null, ErrorType.NotFound);
                }

                var attachments = await _dbContext.LessonAttachments
                    .AsNoTracking()
                    .Where(a => a.LessonId == lessonId && !a.IsDeleted)
                    .ToListAsync(cancellationToken);

                var dtos = attachments.Select(a => new LessonAttachmentDetailsDto
                {
                    AttachmentId = a.Id,
                    LessonId = a.LessonId,
                    LessonName = lesson.Name,
                    AttachmentUrl = a.FileUrl,
                    OpenCount = a.OpenCount,
                    CreatedAt = a.CreatedAt ?? now,
                    UpdatedAt = a.UpdatedAt ?? now
                }).ToList();

                _logger.LogInformation("LessonAttachmentService - GetAttachmentsByLessonIdAsync : {Count} attachments retrieved for lesson {LessonId}.", dtos.Count, lessonId);
                return new GeneralResult<List<LessonAttachmentDetailsDto>>(
                    true, _messages.MsgAttachmentsRetrieved, dtos, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LessonAttachmentService - GetAttachmentsByLessonIdAsync : Error retrieving attachments for lesson {LessonId}.", lessonId);
                return new GeneralResult<List<LessonAttachmentDetailsDto>>(
                    false, _messages.GetUnexpectedErrorMessage("retrieving lesson attachments"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> IncrementOpenCountAsync(int attachmentId, CancellationToken cancellationToken)
        {
            try
            {
                var attachment = await _dbContext.LessonAttachments
                    .FirstOrDefaultAsync(a => a.Id == attachmentId && !a.IsDeleted, cancellationToken);

                if (attachment == null)
                {
                    _logger.LogWarning("LessonAttachmentService - IncrementOpenCountAsync : Attachment with Id {AttachmentId} not found.", attachmentId);
                    return new GeneralResult(false, _messages.MsgAttachmentNotFound, null, ErrorType.NotFound);
                }

                attachment.OpenCount += 1;
                attachment.UpdatedAt = DateTimeOffset.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("LessonAttachmentService - IncrementOpenCountAsync : Open count incremented for attachment {AttachmentId}.", attachmentId);
                return new GeneralResult(true, _messages.MsgOpenCountIncremented, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LessonAttachmentService - IncrementOpenCountAsync : Error incrementing open count for attachment {AttachmentId}.", attachmentId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("incrementing open count"), null, ErrorType.InternalServerError);
            }
        }
    }
}

namespace Lumora.Application.Services.Programs
{
    public class LessonAttachmentService(
        ILessonAttachmentRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<LessonAttachmentService> logger,
        LessonAttachmentMessage messages) : ILessonAttachmentService
    {
        private readonly ILessonAttachmentRepository _repository = repository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<LessonAttachmentService> _logger = logger;
        private readonly LessonAttachmentMessage _messages = messages;

        /// <inheritdoc />
        public async Task<GeneralResult> AddSingleAttachmentAsync(SingleLessonAttachmentCreateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                if (!await _repository.LessonExistsAsync(dto.LessonId, cancellationToken))
                {
                    _logger.LogError("LessonAttachmentService: Lesson {LessonId} not found.", dto.LessonId);
                    return new GeneralResult(false, _messages.MsgLessonNotFound, null, ErrorType.NotFound);
                }

                var attachment = _mapper.Map<LessonAttachment>(dto);
                await _repository.AddAsync(attachment, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new GeneralResult(true, _messages.MsgAttachmentAdded, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding attachment to lesson {LessonId}.", dto.LessonId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("add single attachment"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult> UpdateAttachmentAsync(int attachmentId, LessonAttachmentUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var attachment = await _repository.GetByIdAsync(attachmentId, false, cancellationToken);
                if (attachment == null)
                    return new GeneralResult(false, _messages.MsgAttachmentNotFound, null, ErrorType.NotFound);

                if (dto.LessonId.HasValue)
                {
                    if (!await _repository.LessonExistsAsync(dto.LessonId.Value, cancellationToken))
                        return new GeneralResult(false, _messages.MsgLessonNotFound, null, ErrorType.NotFound);

                    attachment.LessonId = dto.LessonId.Value;
                }

                if (!string.IsNullOrWhiteSpace(dto.FileUrl))
                    attachment.FileUrl = dto.FileUrl;

                attachment.UpdatedAt = DateTimeOffset.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new GeneralResult(true, _messages.MsgAttachmentUpdated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating attachment {AttachmentId}.", attachmentId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("updating attachment"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult> DeleteSingleAttachmentAsync(int attachmentId, CancellationToken cancellationToken)
        {
            try
            {
                var attachment = await _repository.GetByIdAsync(attachmentId, false, cancellationToken);
                if (attachment == null) return new GeneralResult(false, _messages.MsgAttachmentNotFound, null, ErrorType.NotFound);

                var now = DateTimeOffset.UtcNow;
                attachment.IsDeleted = true;
                attachment.DeletedAt = now;
                attachment.UpdatedAt = now;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return new GeneralResult(true, _messages.MsgAttachmentDeleted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {AttachmentId}", attachmentId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("delete attachment."), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult<LessonAttachmentDetailsDto>> GetAttachmentByIdAsync(int attachmentId, CancellationToken cancellationToken)
        {
            try
            {
                var attachment = await _repository.GetByIdAsync(attachmentId, true, cancellationToken);
                if (attachment == null)
                    return new GeneralResult<LessonAttachmentDetailsDto>(false, _messages.MsgAttachmentNotFound, null, ErrorType.NotFound);

                var dto = new LessonAttachmentDetailsDto
                {
                    AttachmentId = attachment.Id,
                    LessonId = attachment.LessonId,
                    LessonName = attachment.CourseLesson?.Name ?? "N/A",
                    AttachmentUrl = attachment.FileUrl,
                    OpenCount = attachment.OpenCount,
                    CreatedAt = attachment.CreatedAt ?? DateTimeOffset.UtcNow,
                    UpdatedAt = attachment.UpdatedAt ?? DateTimeOffset.UtcNow
                };

                return new GeneralResult<LessonAttachmentDetailsDto>(true, _messages.MsgAttachmentRetrieved, dto, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attachment {AttachmentId}", attachmentId);
                return new GeneralResult<LessonAttachmentDetailsDto>(false, _messages.GetUnexpectedErrorMessage("retrieving attachment"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult<List<LessonAttachmentDetailsDto>>> GetAttachmentsByLessonIdAsync(int lessonId, CancellationToken cancellationToken)
        {
            try
            {
                // نحتاج للتأكد من وجود الدرس أولاً (Business Rule)
                if (!await _repository.LessonExistsAsync(lessonId, cancellationToken))
                    return new GeneralResult<List<LessonAttachmentDetailsDto>>(false, _messages.MsgLessonNotFound, null, ErrorType.NotFound);

                var attachments = await _repository.GetByLessonIdAsync(lessonId, cancellationToken);
                var now = DateTimeOffset.UtcNow;

                var dtos = attachments.Select(a => new LessonAttachmentDetailsDto
                {
                    AttachmentId = a.Id,
                    LessonId = a.LessonId,
                    AttachmentUrl = a.FileUrl,
                    OpenCount = a.OpenCount,
                    CreatedAt = a.CreatedAt ?? now,
                    UpdatedAt = a.UpdatedAt ?? now
                }).ToList();

                return new GeneralResult<List<LessonAttachmentDetailsDto>>(true, _messages.MsgAttachmentsRetrieved, dtos, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attachments for lesson {LessonId}.", lessonId);
                return new GeneralResult<List<LessonAttachmentDetailsDto>>(false, _messages.GetUnexpectedErrorMessage("retrieving lesson attachments"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<GeneralResult> IncrementOpenCountAsync(int attachmentId, CancellationToken cancellationToken)
        {
            try
            {
                var attachment = await _repository.GetByIdAsync(attachmentId, false, cancellationToken);
                if (attachment == null)
                    return new GeneralResult(false, _messages.MsgAttachmentNotFound, null, ErrorType.NotFound);

                attachment.OpenCount += 1;
                attachment.UpdatedAt = DateTimeOffset.UtcNow;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return new GeneralResult(true, _messages.MsgOpenCountIncremented, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing open count for attachment {AttachmentId}.", attachmentId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("incrementing open count"), null, ErrorType.InternalServerError);
            }
        }
    }
}

using Lumora.Interfaces.ProgramIntf;
using Lumora.Interfaces.QueriesIntf;

namespace Lumora.Services.Programs
{
    public class TrainingProgramService(
        PgDbContext dbContext,
        ILogger<TrainingProgramService> logger,
        IMapper mapper,
        TrainingProgramMessage messages,
        IQueryService queryService) : ITrainingProgramService
    {
        private readonly TrainingProgramMessage _messages = messages;

        /// <inheritdoc/>
        public async Task<GeneralResult> CreateProgramAsync(TrainingProgramCreateDto dto, CancellationToken cancellationToken)
        {
            var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                if(dto == null)
                {
                    logger.LogWarning("TrainingProgramService - CreateProgramAsync: Received null DTO.");
                    return new GeneralResult(false, _messages.MsgNullOrEmpty, null, ErrorType.BadRequest);
                }

                if (dto.ProgramCourses == null || !dto.ProgramCourses.Any())
                {
                    logger.LogWarning("TrainingProgramService - CreateProgramAsync: Received program with no courses.");
                    return new GeneralResult(false, _messages.MsgProgramNoCourses, null, ErrorType.BadRequest);
                }

                if (dto.ProgramCourses.Exists(c => c.Lessons == null || !c.Lessons.Any()))
                {
                    logger.LogWarning("TrainingProgramService - CreateProgramAsync: Received program with no lessons.");
                    return new GeneralResult(false, _messages.MsgProgramNoLessons, null, ErrorType.BadRequest);
                }

                if (dto.ProgramCourses.Exists(c => c.Lessons != null && c.Lessons.Exists(l => l.LessonAttachments == null || !l.LessonAttachments.Any())))
                {
                    logger.LogWarning("TrainingProgramService - CreateProgramAsync: Received program with no attachments.");
                    return new GeneralResult(false, _messages.MsgProgramNoAttachments, null, ErrorType.BadRequest);
                }

                var entity = mapper.Map<TrainingProgram>(dto);
                foreach (var course in entity.ProgramCourses)
                {
                    course.TrainingProgram = entity;

                    foreach (var lesson in course.Lessons)
                    {
                        lesson.ProgramCourse = course;

                        foreach (var attachment in lesson.LessonAttachments)
                        {
                            attachment.CourseLesson = lesson;
                        }
                    }
                }

                dbContext.TrainingPrograms.Add(entity);
                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync();

                logger.LogInformation("TrainingProgramService - CreateProgramAsync : Program created successfully with ID {Id}.", entity.Id);
                return new GeneralResult(true, _messages.MsgProgramCreated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "TrainingProgramService - CreateProgramAsync : Error creating program");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Creating program"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> UpdateProgramAsync(int programId, TrainingProgramUpdateDto dto, CancellationToken cancellationToken)
        {
            try
            {
                if (dto == null)
                {
                    logger.LogWarning("TrainingProgramService - UpdateProgramAsync: Received null DTO.");
                    return new GeneralResult(false, _messages.MsgNullOrEmpty, null, ErrorType.BadRequest);
                }

                var result = await dbContext.TrainingPrograms.FindAsync(new object[] { programId }, cancellationToken);
                if (result == null || result.IsDeleted)
                {
                    logger.LogWarning("TrainingProgramService - UpdateProgramAsync: Program not found or deleted. ID={ProgramId}", programId);
                    return new GeneralResult(false, _messages.MsgProgramNotFound, null, ErrorType.NotFound);
                }

                // Update fields
                if (dto.Name != null) result.Name = dto.Name;
                if (dto.Description != null) result.Description = dto.Description;
                if (dto.Price.HasValue) result.Price = dto.Price.Value;
                if (dto.Discount.HasValue) result.Discount = dto.Discount.Value;
                if (dto.Logo != null) result.Logo = dto.Logo;
                if (dto.HasCertificateExpiration.HasValue) result.HasCertificateExpiration = dto.HasCertificateExpiration.Value;
                if (dto.CertificateValidityInMonth.HasValue) result.CertificateValidityInMonth = dto.CertificateValidityInMonth.Value;

                if (dto.Audience != null) result.Audience = dto.Audience;
                if (dto.Requirements != null) result.Requirements = dto.Requirements;
                if (dto.Topics != null) result.Topics = dto.Topics;
                if (dto.Goals != null) result.Goals = dto.Goals;
                if (dto.Outcomes != null) result.Outcomes = dto.Outcomes;
                if (dto.Trainers != null) result.Trainers = dto.Trainers;

                result.UpdatedAt = dto.UpdatedAt ?? DateTimeOffset.UtcNow;

                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("TrainingProgramService - UpdateProgramAsync: Program ID={ProgramId} updated successfully.", programId);
                return new GeneralResult(true, _messages.MsgProgramUpdated, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "TrainingProgramService - UpdateProgramAsync: Error updating program ID={ProgramId}", programId);
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("updating program"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> DeleteProgramAsync(int programId, CancellationToken cancellationToken)
        {
            try
            {
                var program = await dbContext.TrainingPrograms.FirstOrDefaultAsync(p => p.Id == programId && !p.IsDeleted, cancellationToken);
                if (program == null) return new GeneralResult(false, _messages.MsgProgramNotFound, null, ErrorType.NotFound);

                var hasCourses = await dbContext.ProgramCourses.AnyAsync(c => c.ProgramId == programId && !c.IsDeleted, cancellationToken);
                if (hasCourses) return new GeneralResult(false, _messages.MsgCannotDeleteProgram, null, ErrorType.BadRequest);

                program.IsDeleted = true;
                program.DeletedAt = DateTimeOffset.UtcNow;

                await dbContext.SaveChangesAsync();
                logger.LogInformation("TrainingProgramService - DeleteSingleProgramAsync : Program deleted successfully.");
                return new GeneralResult(true, _messages.MsgProgramDeleted, null, ErrorType.Success);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "TrainingProgramService - DeleteSingleProgramAsync : Error deleting program.");
                return new GeneralResult(false, _messages.GetUnexpectedErrorMessage("Deleting program"), null, ErrorType.InternalServerError);
            }
        }

                                            /***************** GET METHODS *****************/

        /// <inheritdoc/>
        public async Task<GeneralResult<ProgramCompletionData>> ProgramCompletionRateAsync(int programId, string userId, CancellationToken cancellationToken)
        {
            try
            {
                var completionReport = await dbContext.TraineeProgresses.AsNoTracking()
                    .Where(p => p.ProgramId == programId && p.UserId == userId && !p.IsDeleted)
                    .Select(d => new ProgramCompletionData
                    {
                        IsCompleted = d.IsCompleted,
                        CompletionPercentage = d.CompletionPercentage,
                        TotalTimeSpent = d.TotalTimeSpent
                    }).FirstOrDefaultAsync(cancellationToken);

                if (completionReport == null)
                    return new GeneralResult<ProgramCompletionData>(false, _messages.MsgCompletionReportNotFound, null, ErrorType.NotFound);

                logger.LogInformation("TrainingProgramService - ProgramCompletionRateAsync : Completion report retrieved successfully.");
                return new GeneralResult<ProgramCompletionData>(true, _messages.MsgCompletionReportRetrieved, completionReport, ErrorType.Success);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "TrainingProgramService - ProgramCompletionRateAsync : Error calculating completion rate");
                return new GeneralResult<ProgramCompletionData>(
                    false, _messages.GetUnexpectedErrorMessage("Calculating completion rate"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<TrainingProgramFullDetailsDto>>> GetAllProgramsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var result = await dbContext.TrainingPrograms.AsNoTracking().Where(p => !p.IsDeleted)
                    .Include(p => p.ProgramCourses.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Lessons.Where(l => !l.IsDeleted))
                    .ThenInclude(l => l.LessonAttachments.Where(a => !a.IsDeleted))
                    .Include(p => p.ProgramCourses.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Lessons.Where(l => !l.IsDeleted)).ToListAsync(cancellationToken);

                if (!result.Any())
                {
                    logger.LogInformation("TrainingProgramService - GetAllProgramsAsync : No programs found.");
                    return new GeneralResult<List<TrainingProgramFullDetailsDto>>(false, _messages.MsgProgramNotFound, null, ErrorType.NotFound);
                }

                var programs = result.Select(queryService.MapToProgramDetailsDto).ToList();
                return new GeneralResult<List<TrainingProgramFullDetailsDto>>(true, _messages.MsgProgramsRetrieved, programs, ErrorType.Success);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "TrainingProgramService - GetAllProgramsAsync : Error retrieving programs");
                return new GeneralResult<List<TrainingProgramFullDetailsDto>>(
                    false, _messages.GetUnexpectedErrorMessage("Retrieving programs"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<TrainingProgramFullDetailsDto>> GetOneProgramAsync(int programId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await dbContext.TrainingPrograms.AsNoTracking().Where(p => p.Id == programId && !p.IsDeleted)
                    .Include(p => p.ProgramCourses.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Lessons.Where(l => !l.IsDeleted))
                    .ThenInclude(l => l.LessonAttachments.Where(a => !a.IsDeleted))
                    .Include(p => p.ProgramCourses.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Lessons.Where(l => !l.IsDeleted)).FirstOrDefaultAsync(cancellationToken);
                if (result == null)
                    return new GeneralResult<TrainingProgramFullDetailsDto>(false, _messages.MsgProgramNotFound, null, ErrorType.NotFound);

                var program = queryService.MapToProgramDetailsDto(result);
                logger.LogInformation("TrainingProgramService - GetOneProgramAsync : Program retrieved successfully.");
                return new GeneralResult<TrainingProgramFullDetailsDto>(true, _messages.MsgProgramRetrieved, program, ErrorType.Success);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "TrainingProgramService - GetOneProgramAsync : Error retrieving program");
                return new GeneralResult<TrainingProgramFullDetailsDto>(
                    false, _messages.GetUnexpectedErrorMessage("Retrieving program"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<CourseFullDetailsDto>>> GetProgramCoursesAsync(int programId, CancellationToken cancellationToken)
        {
            try
            {
                var courses = await dbContext.ProgramCourses.Where(c => c.ProgramId == programId && !c.IsDeleted).AsNoTracking()
                    .Include(c => c.Lessons!).ThenInclude(l => l.LessonAttachments)
                    .Include(c => c.Lessons!).ThenInclude(l => l.LessonTest)
                    .ToListAsync();

                if (!courses.Any())
                    return new GeneralResult<List<CourseFullDetailsDto>>(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);

                var result = courses.Select(queryService.MapToCourseDetailsDto).ToList();

                if (!result.Any())
                    return new GeneralResult<List<CourseFullDetailsDto>>(false, _messages.MsgCourseNotFound, null, ErrorType.NotFound);

                logger.LogInformation("TrainingProgramService - GetProgramCoursesAsync : Courses retrieved successfully.");
                return new GeneralResult<List<CourseFullDetailsDto>>(true, _messages.MsgCoursesRetrieved, result, ErrorType.Success);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "TrainingProgramService - GetProgramCoursesAsync : Error retrieving program courses");
                return new GeneralResult<List<CourseFullDetailsDto>>(
                    false, _messages.GetUnexpectedErrorMessage("Retrieving program courses"), null, ErrorType.InternalServerError);
            }
        }
    }
}

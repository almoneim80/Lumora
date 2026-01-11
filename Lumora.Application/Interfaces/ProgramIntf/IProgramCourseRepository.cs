namespace Lumora.Application.Interfaces.ProgramIntf
{
    public interface IProgramCourseRepository
    {
        Task AddCourseAsync(ProgramCourse course, CancellationToken ct);
        Task AddLessonsRangeAsync(IEnumerable<CourseLesson> lessons, CancellationToken ct);
        Task AddAttachmentsRangeAsync(IEnumerable<LessonAttachment> attachments, CancellationToken ct);
        Task AddTestsRangeAsync(IEnumerable<Test> tests, CancellationToken ct);
        Task AddQuestionsRangeAsync(IEnumerable<TestQuestion> questions, CancellationToken ct);
        Task AddChoicesRangeAsync(IEnumerable<TestChoice> choices, CancellationToken ct);

        Task<ProgramCourse?> GetByIdAsync(int id, CancellationToken ct, bool asNoTracking = false);
        Task<TrainingProgram?> GetProgramByIdAsync(int id, CancellationToken ct);
    }
}

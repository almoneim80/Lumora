namespace Lumora.Application.Interfaces.QueriesIntf
{
    public interface IQueryService
    {
        TrainingProgramFullDetailsDto MapToProgramDetailsDto(TrainingProgram program);
        CourseFullDetailsDto MapToCourseDetailsDto(ProgramCourse course);
        LessonFullDetailsDto MapToLessonDetailsDto(CourseLesson lesson);
        TestDetailsDto MapToTestDetailsDto(Test test);
    }
}

namespace Lumora.Application.Interfaces.ProgramIntf
{
    public interface IProgressRepository
    {
        // --- Lesson Operations ---

        /// <summary>
        /// الحصول على درس معين مع تضمين الكورس والبرنامج التابع له.
        /// </summary>
        Task<CourseLesson?> GetLessonWithProgramAsync(int lessonId, CancellationToken cancellationToken);

        /// <summary>
        /// الحصول على سجل تقدم المستخدم في درس معين.
        /// </summary>
        Task<LessonProgress?> GetLessonProgressAsync(string userId, int lessonId, CancellationToken cancellationToken);

        /// <summary>
        /// الحصول على سجل تقدم درس معين مع كافة التفاصيل (تتبع فقط).
        /// </summary>
        Task<LessonProgress?> GetLessonProgressDetailsAsync(string userId, int lessonId, CancellationToken cancellationToken);

        /// <summary>
        /// الحصول على كافة الدروس المكتملة للمستخدم داخل كورس معين.
        /// </summary>
        Task<List<LessonProgress>> GetCompletedLessonsInCourseAsync(string userId, List<int> lessonIds, CancellationToken cancellationToken);

        /// <summary>
        /// إضافة سجل تقدم جديد لدرس.
        /// </summary>
        Task AddLessonProgressAsync(LessonProgress progress, CancellationToken cancellationToken);


        // --- Course Operations ---

        /// <summary>
        /// الحصول على كورس معين مع دروسه (تتبع فقط).
        /// </summary>
        Task<ProgramCourse?> GetCourseWithLessonsAsync(int courseId, CancellationToken cancellationToken);

        /// <summary>
        /// الحصول على كورس معين مع البرنامج التابع له (تتبع فقط).
        /// </summary>
        Task<ProgramCourse?> GetCourseWithProgramAsync(int courseId, CancellationToken cancellationToken);

        /// <summary>
        /// الحصول على قائمة بمعرفات الدروس المكتملة للمستخدم في كورس معين.
        /// </summary>
        Task<List<int>> GetCompletedLessonIdsAsync(string userId, List<int> lessonIds, CancellationToken cancellationToken);

        /// <summary>
        /// الحصول على سجل تقدم متدرب (TraineeProgress) بناءً على مستوى معين (Course/Program).
        /// </summary>
        Task<TraineeProgress?> GetTraineeProgressAsync(string userId, int? courseId, int? programId, ProgressLevel level, CancellationToken cancellationToken);

        /// <summary>
        /// الحصول على كافة سجلات التقدم للكورسات الخاصة بمستخدم معين.
        /// </summary>
        Task<List<TraineeProgress>> GetUserCoursesProgressListAsync(string userId, CancellationToken cancellationToken);

        Task<List<TraineeProgress>> GetUserProgramsProgressListAsync(string userId, CancellationToken cancellationToken);

        // --- Program & Enrollment Operations ---

        /// <summary>
        /// الحصول على برنامج تدريبي معين (تتبع فقط).
        /// </summary>
        Task<TrainingProgram?> GetProgramByIdAsync(int programId, CancellationToken cancellationToken);

        /// <summary>
        /// الحصول على كافة معرفات الكورسات التابعة لبرنامج معين.
        /// </summary>
        Task<List<int>> GetProgramCourseIdsAsync(int programId, CancellationToken cancellationToken);

        /// <summary>
        /// الحصول على سجلات تقدم الكورسات للمستخدم داخل برنامج معين.
        /// </summary>
        Task<List<TraineeProgress>> GetCourseProgressesInProgramAsync(string userId, List<int> programCourseIds, CancellationToken cancellationToken);

        /// <summary>
        /// الحصول على سجل اشتراك مستخدم في برنامج معين بحالة نشطة.
        /// </summary>
        Task<ProgramEnrollment?> GetActiveProgramEnrollmentAsync(int programId, string userId, CancellationToken cancellationToken);


        // --- Shared & General Operations ---

        /// <summary>
        /// الحصول على بيانات كورس مباشر (Live Course).
        /// </summary>
        Task<LiveCourse?> GetLiveCourseByIdAsync(int courseId, CancellationToken cancellationToken);

        /// <summary>
        /// إضافة سجل تقدم متدرب جديد.
        /// </summary>
        Task AddTraineeProgressAsync(TraineeProgress progress, CancellationToken cancellationToken);

        /// <summary>
        /// الحصول على قائمة سجلات التقدم لمجموعة من الكورسات لمستخدم معين.
        /// </summary>
        Task<List<TraineeProgress>> GetCourseProgressesByIdsAsync(string userId, List<int> courseIds, CancellationToken cancellationToken);

        /// <summary>
        /// الحصول على جلسة درس نشطة حالياً لمستخدم.
        /// </summary>
        Task<LessonSession?> GetActiveLessonSessionAsync(string userId, int lessonId, CancellationToken cancellationToken);

        /// <summary>
        /// إضافة سجل جلسة درس جديدة.
        /// </summary>
        Task AddLessonSessionAsync(LessonSession session, CancellationToken cancellationToken);

        /// <summary>
        /// الحصول على مجموع الوقت المستغرق في دروس كورس معين (Ticks).
        /// </summary>
        Task<long> GetTotalTimeSpentInCourseLessonsAsync(string userId, int courseId, CancellationToken cancellationToken);

        /// <summary>
        /// الحصول على مجموع الوقت المستغرق في كافة كورسات برنامج معين (Ticks).
        /// </summary>
        Task<long> GetTotalTimeSpentInProgramCoursesAsync(string userId, List<int> courseIds, CancellationToken cancellationToken);

        /// <summary>
        /// الحصول على كافة المستخدمين المسجلين في برنامج معين.
        /// </summary>
        Task<List<string>> GetEnrolledUserIdsInProgramAsync(int programId, CancellationToken cancellationToken);
    }
}

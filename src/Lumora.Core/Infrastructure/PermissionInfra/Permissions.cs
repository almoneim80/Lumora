namespace Lumora.Infrastructure.PermissionInfra
{
    public static class Permissions
    {
        public static List<string> All => GetAllPermissions();
        private static List<string> GetAllPermissions()
        {
            var permissionType = typeof(Permissions);
            var permissions = new List<string>();

            foreach (var nestedType in permissionType.GetNestedTypes())
            {
                var fields = nestedType.GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var field in fields)
                {
                    var value = field.GetValue(null)?.ToString();
                    if (!string.IsNullOrEmpty(value))
                        permissions.Add(value);
                }
            }

            return permissions;
        }

        // PERMISSIONS
        public static class MerchantPermissions
        {
            public const string Register = "Lumora.Merchant.Register";
            public const string CreateProduct = "Lumora.Merchant.CreateProduct";
            public const string RequestProductUpdate = "Lumora.Merchant.RequestProductUpdate";
            public const string ViewApprovedProducts = "Lumora.Merchant.ViewApprovedProducts";
            public const string ViewProductsByStatus = "Lumora.Merchant.ViewProductsByStatus";
            public const string ViewDashboard = "Lumora.Merchant.ViewDashboard";
            public const string ReviewPayouts = "Lumora.Merchant.ReviewPayouts";
        }

        public static class AffiliatePermissions
        {
            public const string Register = "Lumora.Affiliate.Register";
            public const string ViewDashboard = "Lumora.Affiliate.ViewDashboard";
            public const string GetReferralLink = "Lumora.Affiliate.GetReferralLink";
            public const string RequestPayout = "Lumora.Affiliate.RequestPayout";
            public const string ViewPayoutHistory = "Lumora.Affiliate.ViewPayoutHistory";
            public const string ViewCommissions = "Lumora.Affiliate.ViewCommissions";
        }

        public static class PostLikePermissions
        {
            public const string Like = "Lumora.PostLike.Like";
            public const string Unlike = "Lumora.PostLike.Unlike";
            public const string HasLiked = "Lumora.PostLike.HasLiked";
            public const string GetLikeCount = "Lumora.PostLike.GetLikeCount";
        }

        public static class PostPermissions
        {
            public const string Create = "Lumora.Post.Create";
            public const string Delete = "Lumora.Post.Delete";
            public const string GetById = "Lumora.Post.GetById";
            public const string GetAllPublic = "Lumora.Post.GetAllPublic";
            public const string GetAllByUser = "Lumora.Post.GetAllByUser";
        }

        public static class AmbassadorPermissions
        {
            public const string GetCurrent = "Lumora.Ambassador.GetCurrent";
            public const string GetHistory = "Lumora.Ambassador.GetHistory";
        }

        public static class StaticContentPermissions
        {
            [SuperAdminOnly]
            public const string Create = "Lumora.StaticContent.Create";
            [SuperAdminOnly]
            public const string Edit = "Lumora.StaticContent.Edit";
            [SuperAdminOnly]
            public const string Delete = "Lumora.StaticContent.Delete";

            [SuperAdminOnly]
            public const string GetStaticContentMediaType = "Lumora.StaticContent.GetStaticContentMediaType";

            public const string GetStaticContentType = "Lumora.StaticContent.GetStaticContentType";
        }

        public static class WheelPlayerPermissions
        {
            public const string Spin = "Lumora.WheelPlayerPermissions.Spin";
            public const string CanPlay = "Lumora.WheelPlayerPermissions.CanPlay";
            public const string TodaySpin = "Lumora.WheelPlayerPermissions.TodaySpin";
            public const string History = "Lumora.WheelPlayerPermissions.History";
        }

        public static class WheelAwardPermissions
        {
            public const string GetAll = "Lumora.WheelAward.GetAll";
        }

        public static class LiveCoursePermissions
        {
            public const string GetMyCourses = "Lumora.LiveCourse.GetMyCourses";
            public const string GetById = "Lumora.LiveCourse.GetById";
            public const string GetAll = "Lumora.LiveCourse.GetAll";
            public const string SubscribeUser = "Lumora.LiveCourse.SubscribeUser";
        }

        public static class ProgressPermissions
        {
            public const string MarkLessonCompleted = "Lumora.Progress.MarkLessonCompleted";
            public const string GetLessonProgress = "Lumora.Progress.GetLessonProgress";
            public const string GetCompletedLessons = "Lumora.Progress.GetCompletedLessons";
            public const string UpdateCourseProgress = "Lumora.Progress.UpdateCourseProgress";
            public const string GetCourseProgress = "Lumora.Progress.GetCourseProgress";
            public const string GetLiveCourseProgress = "Lumora.Progress.GetLiveCourseProgress";
            public const string GetUserCoursesProgress = "Lumora.Progress.GetUserCoursesProgress";
            public const string UpdateProgramProgress = "Lumora.Progress.UpdateProgramProgress";
            public const string GetProgramProgress = "Lumora.Progress.GetProgramProgress";
            public const string GetUserProgramsProgress = "Lumora.Progress.GetUserProgramsProgress";
            public const string StartLessonSession = "Lumora.Progress.StartLessonSession";
            public const string EndLessonSession = "Lumora.Progress.EndLessonSession";
            public const string SyncProgramProgress = "Lumora.Progress.SyncProgramProgress";
        }

        public static class ProgramsManegmentPermissions
        {
            public const string GetAll = "Lumora.Programs.GetAll";
            public const string GetOne = "Lumora.Programs.GetOne";
            public const string CompletionRate = "Lumora.Programs.CompletionRate";
            public const string GetCourses = "Lumora.Programs.GetCourses";
        }

        public static class ProgramEnrollmentPermissions
        {
            public const string Enroll = "Lumora.ProgramEnrollment.Enroll";
            public const string Unenroll = "Lumora.ProgramEnrollment.Unenroll";
            public const string CheckEnrollment = "Lumora.ProgramEnrollment.CheckEnrollment";
            public const string GetEnrollmentInfo = "Lumora.ProgramEnrollment.GetEnrollmentInfo";
        }

        public static class ProgramCoursePermissions
        {
            public const string ViewCourseDetails = "Lumora.ProgramCourse.ViewCourseDetails";
        }

        public static class LessonPermissions
        {
            public const string ViewLessonsByCourse = "Lumora.Lesson.ViewLessonsByCourse";
        }

        public static class LessonAttachmentPermissions
        {
            public const string ViewAttachmentById = "Lumora.LessonAttachment.ViewAttachmentById";
            public const string ViewAttachmentsByLesson = "Lumora.LessonAttachment.ViewAttachmentsByLesson";
            public const string IncrementOpenCount = "Lumora.LessonAttachment.IncrementOpenCount";
        }

        public static class CertificatePermissions
        {
            public const string Issue = "Lumora.Certificate.Issue";
            public const string ViewById = "Lumora.Certificate.ViewById";
            public const string ViewUserCertificates = "Lumora.Certificate.ViewUserCertificates";
            public const string ExportPdf = "Lumora.Certificate.ExportPdf";
            public const string Verify = "Lumora.Certificate.Verify";
        }

        public static class TestPermissions
        {
            public const string GetTestById = "Lumora.Test.GetTestById";
        }
        public static class TestAttemptPermissions
        {
            public const string StartAttempt = "Lumora.Test.StartAttempt";
            public const string SubmitAnswer = "Lumora.Test.SubmitAnswer";
            public const string SubmitAttempt = "Lumora.Test.SubmitAttempt";
            public const string GetResult = "Lumora.Test.GetResult";
            public const string GetMyAttempts = "Lumora.Test.GetMyAttempts";
        }

        public static class TestQuestionPermissions
        {
            public const string Create = "Lumora.TestQuestion.Create";
            public const string Update = "Lumora.TestQuestion.Update";
            public const string Delete = "Lumora.TestQuestion.Delete";
            public const string Get = "Lumora.TestQuestion.Get";
            public const string GetByTest = "Lumora.TestQuestion.GetByTest";
        }

        public static class JobPermissions
        {
            public const string GetOne = "Lumora.Job.GetOne";
            public const string GetAll = "Lumora.Job.GetAll";
            public const string Apply = "Lumora.Job.Apply";
            public const string GetUserApplications = "Lumora.Job.GetUserApplications";
        }

        public static class TestChoicePermissions
        {
            public const string Create = "Lumora.TestChoice.Create";
            public const string Update = "Lumora.TestChoice.Update";
            public const string Delete = "Lumora.TestChoice.Delete";
            public const string GetByQuestion = "Lumora.TestChoice.GetByQuestion";
            public const string SetCorrectness = "Lumora.TestChoice.SetCorrectness";
        }

        // #Admin
        public static class ProgramAdminPermissions
        {
            public const string Create = "Lumora.ProgramAdmin.Create";
            public const string Update = "Lumora.ProgramAdmin.Update";
            public const string Delete = "Lumora.ProgramAdmin.Delete";
        }

        public static class ProgramEnrollmentAdminPermissions
        {
            public const string GetEnrolledUsers = "Lumora.ProgramEnrollmentAdmin.GetEnrolledUsers";
            public const string IsUserEnrolled = "Lumora.ProgramEnrollmentAdmin.IsUserEnrolled";
        }

        public static class ProgramCourseAdminPermissions
        {
            public const string Create = "Lumora.ProgramCourseAdmin.Create";
            public const string Update = "Lumora.ProgramCourseAdmin.Update";
            public const string Delete = "Lumora.ProgramCourseAdmin.Delete";
        }

        public static class LessonAttachmentAdminPermissions
        {
            public const string Create = "Lumora.LessonAttachmentAdmin.Create";
            public const string Update = "Lumora.LessonAttachmentAdmin.Update";
            public const string Delete = "Lumora.LessonAttachmentAdmin.Delete";
        }

        public static class LessonAdminPermissions
        {
            public const string Create = "Lumora.LessonAdmin.Create";
            public const string Update = "Lumora.LessonAdmin.Update";
            public const string Delete = "Lumora.LessonAdmin.Delete";
        }

        public static class CertificateAdminPermissions
        {
            public const string CountByProgram = "Lumora.CertificateAdmin.CountByProgram";
            public const string Revoke = "Lumora.CertificateAdmin.Revoke";
            public const string GenerateVerificationCode = "Lumora.CertificateAdmin.GenerateVerificationCode";
        }

        public static class LiveCourseAdminPermissions
        {
            public const string Create = "Lumora.LiveCourseAdmin.Create";
            public const string Update = "Lumora.LiveCourseAdmin.Update";
            public const string Delete = "Lumora.LiveCourseAdmin.Delete";
            public const string GetSubscribers = "Lumora.LiveCourseAdmin.GetSubscribers";
            public const string SetStatus = "Lumora.LiveCourseAdmin.SetStatus";
        }

        public static class PodcastEpisodePermissions
        {
            public const string GetById = "Lumora.PodcastEpisode.GetById";
            public const string GetAll = "Lumora.PodcastEpisode.GetAll";
        }

        public static class PodcastEpisodeAdminPermissions
        {
            public const string Create = "Lumora.PodcastEpisode.Admin.Create";
            public const string Update = "Lumora.PodcastEpisode.Admin.Update";
            public const string Delete = "Lumora.PodcastEpisode.Admin.Delete";
        }

        public static class WheelAdminPermissions
        {
            public const string CreateAward = "Lumora.WheelAdmin.CreateAward";
            public const string UpdateAward = "Lumora.WheelAdmin.UpdateAward";
            public const string GetAwardById = "Lumora.WheelAdmin.GetAwardById";
            public const string DeleteAward = "Lumora.WheelAdmin.DeleteAward";
            public const string GetAwardType = "Lumora.WheelAdmin.GetAwardType";

            public const string ViewAllPlays = "Lumora.WheelAdmin.ViewAllPlays";
            public const string ManageDelivery = "Lumora.WheelAdmin.ManageDelivery";
            public const string ViewDeliveryStatus = "Lumora.WheelAdmin.ViewDeliveryStatus";
            public const string ViewPhysicalDelivery = "Lumora.WheelAdmin.ViewPhysicalDelivery";
            public const string ManagePhysicalDelivery = "Lumora.WheelAdmin.ManagePhysicalDelivery";
        }

        public static class PostAdminPermissions
        {
            public const string ReviewPost = "Lumora.PostAdmin.ReviewPost";
            public const string GetPendingPosts = "Lumora.PostAdmin.GetPendingPosts";
            public const string GetUserPosts = "Lumora.PostAdmin.GetUserPosts";
        }

        public static class AdminAmbassadorPermissions
        {
            public const string Assign = "Lumora.AdminAmbassador.Assign";
            public const string Remove = "Lumora.AdminAmbassador.Remove";
        }

        public static class JobAdminPermissions
        {
            public const string Create = "Lumora.Job.Create";
            public const string Update = "Lumora.Job.Update";
            public const string Delete = "Lumora.Job.Delete";
            public const string ToggleActivation = "Lumora.Job.ToggleActivation";
            public const string UpdateApplicationStatus = "Lumora.Job.UpdateApplicationStatus";
            public const string GetAllApplications = "Lumora.Job.GetAllApplications";
            public const string GetApplicationsByJob = "Lumora.Job.GetApplicationsByJob";
        }

        public static class PromoCodePermissions
        {
            public const string Create = "Lumora.PromoCode.Create";
            public const string RegisterUsage = "Lumora.PromoCode.RegisterUsage";
            public const string DeactivateAll = "Lumora.PromoCode.DeactivateAll";
            public const string ViewReport = "Lumora.PromoCode.ViewReport";
            public const string ViewOwn = "Lumora.PromoCode.ViewOwn";
            public const string Reactivate = "Lumora.PromoCode.Reactivate";
        }
    }
}

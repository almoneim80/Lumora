namespace Lumora.Infrastructure
{
    public static class AppRoles
    {
        public const string AllAdmins = SuperAdmin + "," + Admin;
        public const string AllRoles = SuperAdmin + "," + Admin + "," + User + "," + Guest + "," + Trainer + "," + Affiliate + "," + Merchant;
        public const string MarktingRoles = Affiliate + "," + Merchant;
        public const string AffiliateRoles = Affiliate + "," + SuperAdmin + "," + Admin;
        public const string MerchantRoles = Merchant + "," + SuperAdmin + "," + Admin;
        public const string PostRoles = User + "," + SuperAdmin + "," + Admin;
        public const string WheelRoles = User + "," + SuperAdmin + "," + Admin;
        public const string JobRoles = Student + "," + SuperAdmin + "," + Admin;
        public const string CertificateRoles = Student + "," + SuperAdmin + "," + Admin;
        public const string LessonRoles = Student + "," + SuperAdmin + "," + Admin;
        public const string ProgramCourseRoles = Student + "," + SuperAdmin + "," + Admin;
        public const string ProgramEnrollmentRoles = Student + "," + SuperAdmin + "," + Admin;
        public const string TrainingProgramRoles = Student + "," + SuperAdmin + "," + Admin;
        public const string ProgressRoles = Student + "," + SuperAdmin + "," + Admin;
        public const string TestRoles = Student + "," + SuperAdmin + "," + Admin;
        public const string LiveCourseRoles = Student + "," + Trainer + "," + SuperAdmin + "," + Admin;
        public const string PodcastRoles = User + "," + SuperAdmin + "," + Admin;

        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Guest = "Guest";
        public const string Trainer = "Trainer";
        public const string Student = "Student";
        public const string Affiliate = "Affiliate";
        public const string Merchant = "Merchant";

        public static readonly string[] All = [SuperAdmin, Admin, User, Guest, Trainer, Student, Affiliate, Merchant];
    }
}

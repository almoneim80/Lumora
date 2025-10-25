namespace Lumora.Infrastructure.PermissionInfra
{
    public class PermissionsSeeding
    {
        /// <summary>
        /// Default permissions assigned to roles.
        /// Use "*" to assign all permissions except those marked with [SuperAdminOnly].
        /// </summary>
        private static readonly Dictionary<string, List<string>> DefaultPermissionsByRole = new()
        {
            {
                "SuperAdmin", new List<string>
                {
                    "**"
                }
            },
            {
                "Admin", new List<string>
                {
                     "**"
                }
            },
            {
                "User", new List<string>
                {
                    "Lumora.PostLike.Like",
                    "Lumora.PostLike.Unlike",
                    "Lumora.PostLike.HasLiked",
                    "Lumora.PostLike.GetLikeCount",

                    "Lumora.Post.Create",
                    "Lumora.Post.Delete",
                    "Lumora.Post.GetById",
                    "Lumora.Post.GetAllPublic",
                    "Lumora.Post.GetAllByUser",

                    "Lumora.Ambassador.GetCurrent",
                    "Lumora.Ambassador.GetHistory",

                    "Lumora.StaticContent.GetStaticContentType",

                    "Lumora.WheelPlayerPermissions.Spin",
                    "Lumora.WheelPlayerPermissions.CanPlay",
                    "Lumora.WheelPlayerPermissions.TodaySpin",
                    "Lumora.WheelPlayerPermissions.History",

                    "Lumora.WheelAward.GetAll"
                }
            },
            {
                "Student", new List<string>
                {
                     "Lumora.LiveCourse.GetMyCourses",
                     "Lumora.LiveCourse.GetById",
                     "Lumora.LiveCourse.GetAll",
                     "Lumora.LiveCourse.SubscribeUser",

                     "Lumora.Progress.MarkLessonCompleted",
                     "Lumora.Progress.GetLessonProgress",
                     "Lumora.Progress.GetCompletedLessons",
                     "Lumora.Progress.UpdateCourseProgress",
                     "Lumora.Progress.GetCourseProgress",
                     "Lumora.Progress.GetLiveCourseProgress",
                     "Lumora.Progress.GetUserCoursesProgress",
                     "Lumora.Progress.UpdateProgramProgress",
                     "Lumora.Progress.GetProgramProgress",
                     "Lumora.Progress.GetUserProgramsProgress",
                     "Lumora.Progress.StartLessonSession",
                     "Lumora.Progress.EndLessonSession",
                     "Lumora.Progress.SyncProgramProgress",

                     "Lumora.Programs.GetAll",
                     "Lumora.Programs.GetOne",
                     "Lumora.Programs.CompletionRate",
                     "Lumora.Programs.GetCourses",

                     "Lumora.ProgramEnrollment.Enroll",
                     "Lumora.ProgramEnrollment.Unenroll",
                     "Lumora.ProgramEnrollment.CheckEnrollment",
                     "Lumora.ProgramEnrollment.GetEnrollmentInfo",

                     "Lumora.ProgramCourse.ViewCourseDetails",

                     "Lumora.Lesson.ViewLessonsByCourse",

                     "Lumora.LessonAttachment.ViewAttachmentById",
                     "Lumora.LessonAttachment.ViewAttachmentsByLesson",
                     "Lumora.LessonAttachment.IncrementOpenCount",

                     "Lumora.Certificate.Issue",
                     "Lumora.Certificate.ViewById",
                     "Lumora.Certificate.ViewUserCertificates",
                     "Lumora.Certificate.ExportPdf",
                     "Lumora.Certificate.Verify",

                     "Lumora.Test.GetTestById",
                     "Lumora.Test.StartAttempt",
                     "Lumora.Test.SubmitAnswer",
                     "Lumora.Test.SubmitAttempt",
                     "Lumora.Test.GetResult",
                     "Lumora.Test.GetMyAttempts",

                     "Lumora.TestQuestion.Create",
                     "Lumora.TestQuestion.Update",
                     "Lumora.TestQuestion.Delete",
                     "Lumora.TestQuestion.Get",
                     "Lumora.TestQuestion.GetByTest",

                     "Lumora.TestChoice.Create",
                     "Lumora.TestChoice.Update",
                     "Lumora.TestChoice.Delete",
                     "Lumora.TestChoice.GetByQuestion",
                     "Lumora.TestChoice.SetCorrectness"
                }
            },
            {
                "Affiliate", new List<string>
                {
                     "Lumora.Affiliate.Register",
                     "Lumora.Affiliate.ViewDashboard",
                     "Lumora.Affiliate.GetReferralLink",
                     "Lumora.Affiliate.RequestPayout",
                     "Lumora.Affiliate.ViewPayoutHistory",
                     "Lumora.Affiliate.ViewCommissions"
                }
            },
            {
                "Merchant", new List<string>
                {
                     "Lumora.Merchant.Register",
                     "Lumora.Merchant.CreateProduct",
                     "Lumora.Merchant.RequestProductUpdate",
                     "Lumora.Merchant.ViewApprovedProducts",
                     "Lumora.Merchant.ViewDashboard",
                     "Lumora.Merchant.ReviewPayouts"
                }
            }
        };

        /// <summary>
        /// Assigns default permissions to roles.
        /// </summary>
        public static async Task SeedRolePermissionsAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var (roleName, rawPermissions) in DefaultPermissionsByRole)
            {
                var role = await roleManager.FindByNameAsync(roleName)
                           ?? new IdentityRole(roleName);

                if (role.Id == null)
                {
                    var createResult = await roleManager.CreateAsync(role);
                    if (!createResult.Succeeded)
                        throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }

                var existingClaims = (await roleManager.GetClaimsAsync(role))
                    .Where(c => c.Type == "Permission")
                    .Select(c => c.Value)
                    .ToHashSet();

                List<string> resolvedPermissions;

                if (rawPermissions.Contains("**"))
                {
                    // All permissions including [SuperAdminOnly]
                    resolvedPermissions = GetAllPermissions(includeSuperAdminOnly: true)
                        .Select(p => p.Value)
                        .ToList();
                }
                else if (rawPermissions.Contains("*"))
                {
                    // All permissions without [SuperAdminOnly]
                    resolvedPermissions = GetAllPermissions(includeSuperAdminOnly: false)
                        .Select(p => p.Value)
                        .ToList();
                }
                else
                {
                    // Explicitly Written permissions
                    resolvedPermissions = rawPermissions;
                }

                foreach (var permission in resolvedPermissions.Distinct())
                {
                    if (string.IsNullOrWhiteSpace(permission) || existingClaims.Contains(permission))
                        continue;

                    var addResult = await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
                    if (!addResult.Succeeded)
                        throw new Exception($"Failed to add permission '{permission}' to role '{roleName}'.");
                }
            }
        }

        /// <summary>
        /// get all permissions marked with [SuperAdminOnly].
        /// </summary>
        private static List<string> GetSuperAdminOnlyPermissions()
        {
            return GetAllPermissions(includeSuperAdminOnly: true)
                .Where(p => p.IsSuperAdminOnly)
                .Select(p => p.Value)
                .ToList();
        }

        /// <summary>
        /// get all permissions except admin only.
        /// </summary>
        private static List<string> GetAllNonAdminPermissions()
        {
            return GetAllPermissions(includeSuperAdminOnly: false)
                .Select(p => p.Value)
                .ToList();
        }

        /// <summary>
        /// get all permissions.
        /// </summary>
        private static List<(string Value, bool IsSuperAdminOnly)> GetAllPermissions(bool includeSuperAdminOnly)
        {
            var result = new List<(string, bool)>();
            var nestedTypes = typeof(Permissions).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

            foreach (var type in nestedTypes)
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                 .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string));

                foreach (var field in fields)
                {
                    var value = field.GetRawConstantValue() as string;
                    if (string.IsNullOrWhiteSpace(value))
                        continue;

                    var isSuperAdminOnly = field.GetCustomAttribute<SuperAdminOnlyAttribute>() != null;

                    if (!includeSuperAdminOnly && isSuperAdminOnly)
                        continue;

                    result.Add((value, isSuperAdminOnly));
                }
            }

            return result;
        }
    }
}

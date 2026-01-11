using Lumora.Application.Interfaces.AuthorizationIntf;
using Lumora.Application.Interfaces.EmailIntf;
using Lumora.Application.Interfaces.Infrastructure;
using Lumora.Application.Interfaces.TokenIntf;
using Lumora.Application.Services;
using Lumora.Infrastructure.Printing;
using Lumora.Infrastructure.Services.ExternalServices.StaticContent;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Lumora.Web.Extensions
{
    public static class ServiceRegistrationExtensions
    {
        public static void RegisterAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            // configure    
            services.Configure<OtpOptions>(configuration.GetSection("OtpOptions"));
            services.Configure<EmailSenderOptions>(configuration.GetSection("EmailSender"));
            services.Configure<OtpVerificationOptions>(configuration.GetSection("OtpVerification"));
            //services.Configure<PayTabsOptions>(configuration.GetSection("PayTabsSettings"));
            services.Configure<CacheSettings>(configuration.GetSection("CacheSettings"));
            services.Configure<DefaultRolesConfig>(configuration.GetSection("DefaultRoles"));
            services.Configure<FileUploadSettings>(configuration.GetSection("FileUploadSettings"));
            services.Configure<FfmpegSettings>(configuration.GetSection("FfmpegSettings"));


            services.AddScoped<GeneralMessage>();
            services.AddScoped<AuthenticationMessage>();
            services.AddScoped<RoleMessages>();
            services.AddScoped<PermissionMessage>();
            services.AddScoped<CertificateMessages>();
            services.AddScoped<CourseMessage>();
            services.AddScoped<TrainingProgramMessage>();
            services.AddScoped<CourseLessonMessages>();
            services.AddScoped<LessonAttachmentMessage>();
            services.AddScoped<JobMessages>();
            services.AddScoped<TestMessage>();
            services.AddScoped<ProgressMessage>();
            services.AddScoped<PaymentMessage>();
            services.AddScoped<EnrollmentMessage>();
            services.AddScoped<ClubMessage>();
            services.AddScoped<PodcastEpisodeMessage>();
            services.AddScoped<LiveCourseMessage>();
            services.AddScoped<WheelMessag>();
            services.AddScoped<TrackingMessage>();
            services.AddScoped<AffiliateMessage>();

            services.AddScoped<IDomainRepository, WebDomainRepository>();
            services.AddScoped<IWebCheckerService, WebCheckerService>();
            services.AddScoped<IWebDomainService, WebDomainService>();
            services.AddScoped<StaticContentService>();
            services.AddScoped<IStaticContentService>(provider =>
            {
                var originalService = provider.GetRequiredService<StaticContentService>();
                var cache = provider.GetRequiredService<ICacheService>();
                return new StaticContentCacheDecorator(originalService, cache);
            });


            // scoped services
            services.AddScoped<IEmailSend, EmailSendService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IEmailConfirmation, EmailConfirmationService>();
            services.AddScoped<ICascadeDeleteService, CascadeDeleteService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IpDetailsService, IpDetailsService>();
            services.AddScoped<ILockService, LockService>();
            services.AddScoped<IEmailValidationExternalService, EmailValidationExternalService>();
            services.AddScoped<IAccountExternalService, AccountExternalService>();
            services.AddScoped<TaskStatusService, TaskStatusService>();
            services.AddScoped<ActivityLogService, ActivityLogService>();
            services.AddScoped<IEmailVerificationService, EmailVerificationService>();
            services.AddScoped<IEmailVerifyService, EmailVerifyService>();
            services.AddScoped<IEmailVerificationExtension, EmailVerificationExtensionService>();
            services.AddScoped<IHttpContextHelper, HttpContextHelper>();
            services.AddScoped<IVariablesService, VariablesService>();
            services.AddScoped(typeof(IImportService<,>), typeof(ImportService<,>));
            services.AddScoped<IExternalAuthService, GoogleAuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<EsDbContext>();
            services.AddScoped<ICertificateService, CertificateService>();
            services.AddScoped<ITrainingProgramService, TrainingProgramService>();
            services.AddScoped<IProgramCourseService, ProgramCourseService>();
            services.AddScoped<IQueryService, QueryService>();
            services.AddScoped<ICourseLessonService, CourseLessonService>();
            services.AddScoped<ILessonAttachmentService, LessonAttachmentService>();
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<IProgressService, ProgressService>();
            services.AddScoped<IStaticContentService, StaticContentService>();
            services.AddScoped<IExtendedBaseService, ExtendedBaseService>();
            services.AddScoped<IExtendedBaseService, ExtendedBaseService>();
            services.AddHttpClient<IPaymentGatewayAdapter, PayTabsAdapter>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IRefundRepository, RefundRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IEnrollmentService, EnrollmentService>();
            services.AddScoped<IPaymentVerifier, PaymentVerifierService>();
            services.AddScoped<ITestService, TestService>();
            services.AddScoped<ITestChoiceService, TestChoiceService>();
            services.AddScoped<ITestQuestionService, TestQuestionService>();
            services.AddScoped<ITestAttemptService, TestAttemptService>();
            services.AddScoped<IAmbassadorService, AmbassadorService>();
            services.AddScoped<IPostLikeService, PostLikeService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<ILiveCourseService, LiveCourseService>();
            services.AddScoped<IPodcastEpisodeService, PodcastEpisodeService>();
            services.AddScoped<IWheelAwardService, WheelAwardService>();
            services.AddScoped<IWheelPlayerService, WheelPlayerService>();

            services.AddScoped<IAffiliateService, AffiliateService>();
            services.AddScoped<FileValidatorHelper>();
            services.AddScoped<IStaticContentRepository, StaticContentRepository>();
            services.AddScoped<IStaticContentFallbackProvider, StaticContentFallbackProvider>();
            services.AddScoped<IIdentityRepository, IdentityRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserEmailRepository, UserEmailRepository>();
            services.AddScoped<IUnitOfWork, PgDbContext>();
            services.AddScoped<IProgramCourseRepository, ProgramCourseRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<ICascadeRepository, CascadeRepository>();
            services.AddScoped<ILiveCourseRepository, LiveCourseRepository>();
            services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
            services.AddScoped<ICertificateRepository, CertificateRepository>();
            services.AddScoped<IPdfGenerator, IronPdfGenerator>();
            services.AddScoped<ITrainingProgramRepository, TrainingProgramRepository>();
            services.AddScoped<ICourseLessonRepository, CourseLessonRepository>();
            services.AddScoped<ILessonAttachmentRepository, LessonAttachmentRepository>();
            services.AddScoped<IJobRepository, JobRepository>();
            services.AddScoped<IProgressRepository, ProgressRepository>();
            services.AddScoped<IBaseRepository, BaseRepository>();
            services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
            services.AddScoped<ITestRepository, TestRepository>();
            services.AddScoped<IAmbassadorRepository, AmbassadorRepository>();
            services.AddScoped<ITestChoiceRepository, TestChoiceRepository>();
            services.AddScoped<ITestQuestionRepository, TestQuestionRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<ITestAttemptRepository, TestAttemptRepository>();
            services.AddScoped<IPostLikeRepository, PostLikeRepository>();
            services.AddScoped<IPodcastEpisodeRepository, PodcastEpisodeRepository>();
            services.AddScoped<IWheelAwardRepository, WheelAwardRepository>();
            services.AddScoped<IAffiliateRepository, AffiliateRepository>();
            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<IWheelRepository, WheelRepository>();

            // singletons services
            services.AddSingleton<IEmailService, EmailService>();
            services.AddSingleton<ILocalizationManager, LocalizationManager>();
            services.AddSingleton<IServerConfigurationManager, ServerConfigurationManager>();
            services.AddSingleton<ICacheService, MemoryCacheService>();
            services.AddSingleton<ITaskStatusService, TaskStatusService>();


            // transient services
            services.AddTransient<IEmailSchedulingService, EmailSchedulingService>();
            services.AddTransient<IMxVerifyService, MxVerifyService>();
            services.AddTransient<IWebDomainService, WebDomainService>();
            services.AddTransient<IContactService, ContactService>();
            services.AddTransient(typeof(QueryProviderFactory<>), typeof(QueryProviderFactory<>));
            services.AddTransient(typeof(ESOnlyQueryProviderFactory<>), typeof(ESOnlyQueryProviderFactory<>));
        }
    }
}

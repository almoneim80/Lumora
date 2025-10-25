using Lumora.Interfaces.AffiliateMarketingIntf;
using Lumora.Interfaces.Club;
using Lumora.Interfaces.ClubIntf;
using Lumora.Interfaces.JobIntf;
using Lumora.Interfaces.LiveCourseIntf;
using Lumora.Interfaces.PaymentIntf;
using Lumora.Interfaces.PodcastEpisodeIntf;
using Lumora.Interfaces.ProgramIntf;
using Lumora.Interfaces.PrograProgramIntfms;
using Lumora.Interfaces.QueriesIntf;
using Lumora.Interfaces.StaticContentIntf;
using Lumora.Interfaces.TestIntf;
using Lumora.Interfaces.UserIntf;
using Lumora.Services.AffiliateMarketingSvc;
using Lumora.Services.BaseSvc;
using Lumora.Services.Club;
using Lumora.Services.ClubIntf;
using Lumora.Services.ClubSvc;
using Lumora.Services.JobServiceSvc;
using Lumora.Services.LiveCourseSvc;
using Lumora.Services.PaymentSvc;
using Lumora.Services.PaymentSvc.Gateways;
using Lumora.Services.PodcastEpisodeSvc;
using Lumora.Services.QueriesScv;
using Lumora.Services.StaticContentSvc;
using Lumora.Services.TestSvc;
using Lumora.Services.UserSvc;

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
            services.Configure<LocalizationSettings>(configuration.GetSection("LocalizationSettings"));
            services.Configure<DefaultRolesConfig>(configuration.GetSection("DefaultRoles"));
            services.Configure<AffiliateSettings>(configuration.GetSection("Affiliate"));
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

            // singletons services
            services.AddSingleton<IEmailService, EmailService>();
            services.AddSingleton<ILocalizationManager, LocalizationManager>();
            services.AddSingleton<IServerConfigurationManager, ServerConfigurationManager>();
            services.AddSingleton<ICacheService, MemoryCacheService>();

            // transient services
            services.AddTransient<IEmailSchedulingService, EmailSchedulingService>();
            services.AddTransient<IMxVerifyService, MxVerifyService>();
            services.AddTransient<IIdentityService, IdentityService>();
            services.AddTransient<IDomainService, DomainService>();
            services.AddTransient<IContactService, ContactService>();
            services.AddTransient(typeof(QueryProviderFactory<>), typeof(QueryProviderFactory<>));
            services.AddTransient(typeof(ESOnlyQueryProviderFactory<>), typeof(ESOnlyQueryProviderFactory<>));
        }
    }
}

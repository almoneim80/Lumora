<Lumora>

<Lumora> is a comprehensive educational and social platform designed to empower users to learn, track progress, 
and manage courses and content professionally, with support for subscriptions, payments, social interaction, and administrative tasks.

###  Key Features
Course and Learning Program Management
Supports multiple course types: Online, Path-based, Skill Library.
Manage lessons, exercises, tests, and user progress tracking.
Subscription and Payment System
Support for creating and renewing subscriptions.
Link payments to subscriptions and purchases.
Dynamic Content Management
Supports static and dynamic content (text, images, files).
Database-driven content updates without changing code.
Consultation and Support System
Manage counselors, bookings, and tickets.
Messaging and attachments for professional communication.
Diverse Purchase System
Supports purchasing courses, certificates, consultations, and other digital products.
Handles refunds and detailed purchase statistics.
Reports and Analytics
Track user progress.
Comprehensive analytics for admins and supervisors.
Professional Design and Scalability
Extensible architecture.
Localization support (multi-language).
Clear and organized folder structure.

### Folder Structure
Lumora/
├── plugins/
│   └── Lumora.Plugin.Sms/
│       ├── Dependencies/
│       ├── Configuration/
│       │   └── PluginSettings.cs
│       ├── Controllers/
│       │   ├── MessagesController.cs
│       │   └── SmsTemplateController.cs
│       ├── DTOs/
│       │   ├── OTPDtos.cs
│       │   └── SmsDtos.cs
│       ├── Exceptions/
│       │   ├── MsegatException.cs
│       │   ├── SMSPluginException.cs
│       │   └── UnknownCountryCodeException.cs
│       ├── Interfaces/
│       │   ├── IOrpService.cs
│       │   └── ISmsService.cs
│       ├── Services/
│       │   ├── MsegatSmsService.cs
│       │   ├── SmsOrpService.cs
│       │   ├── SmsService.cs
│       │   └── TwilioSmsService.cs
│       ├── Tasks/
│       │   └── SyncSmsLogTasks.cs
│       ├── pluginsettings.json
│       ├── SmsPlugin.cs
│       ├── stylecop.json
│       └── Using.cs
├── Lumora.Core/
│   ├── Configuration/
│   │   ├── MappingProfiles/
│   │   │   ├── MappingConditions.cs
│   │   │   ├── TrainingProgramProfiles.cs
│   │   │   └── UserProfiles.cs
│   │   ├── AppSettings.cs
│   │   ├── AutoMapperProfiles.cs
│   │   ├── RouteTokenabCase.cs
│   │   └── SnakeCaseNamingPolicy.cs
│   ├── Data/
│   │   ├── DbSets/
│   │   │   ├── PgDbContext.AffiliateMarketing.cs
│   │   │   ├── PgDbContext.Club.cs
│   │   │   ├── PgDbContext.Email.cs
│   │   │   ├── PgDbContext.Jobs.cs
│   │   │   ├── PgDbContext.LiveCourse.cs
│   │   │   ├── PgDbContext.Logs.cs
│   │   │   ├── PgDbContext.Payments.cs
│   │   │   ├── PgDbContext.PodcastEpisode.cs
│   │   │   ├── PgDbContext.Programs.cs
│   │   │   ├── PgDbContext.StaticContent.cs
│   │   │   └── PgDbContext.Test.cs
│   │   ├── EsDbContext.cs
│   │   ├── PgDbContext.cs
│   │   └── PluginDbContextBase.cs
│   ├── DataAnnotations/
│   │   ├── CurrencyCodeAttribute.cs
│   │   ├── CustomEmailAttribute.cs
│   │   ├── DateGreaterThanAttribute.cs
│   │   ├── FileExtensionAttribute.cs
│   │   ├── FileExtensionValidateAttribute.cs
│   │   ├── LanguageCodeAttribute.cs
│   │   ├── MediaExtensionAttribute.cs
│   │   ├── RequiredPermissionAttribute.cs
│   │   ├── SearchableAttribute.cs
│   │   ├── SuperAdminOnlyAttribute.cs
│   │   ├── SupportsChangedLogAttribute.cs
│   │   ├── SupportsElasticAttribute.cs
│   │   ├── SurrogateForeignKeyAttribute.cs
│   │   ├── SurrogatedEntityAttribute.cs
│   │   ├── SwaggerExampleAttribute.cs
│   │   ├── SwaggerHideAttribute.cs
│   │   └── SwaggerUniqueAttribute.cs
│   ├── DTOs/
│   │   ├── AffiliateMarketing/
│   │   │   └── AffiliateDtos.cs
│   │   ├── Authentication/
│   │   │   └── AuthenticationDtos.cs
│   │   ├── Authorization/
│   │   │   └── PermissionDtos.cs
│   │   ├── Base/
│   │   │   └── BaseDtos.cs
│   │   ├── Club/
│   │   │   ├── ClubAmbassadorDtos.cs
│   │   │   ├── ClubPostDtos.cs
│   │   │   ├── WheelAwardDto.cs
│   │   │   └── WheelPlayerDto.cs
│   │   ├── Email/
│   │   │   ├── EmailDtos.cs
│   │   │   ├── EmailGroupDtos.cs
│   │   │   └── EmailTemplateDtos.cs
│   │   ├── Job/
│   │   │   └── JobDtos.cs
│   │   ├── LiveCourse/
│   │   │   └── LiveCourseDtos.cs
│   │   ├── MediaDtos/
│   │   │   ├── FileCreateDto.cs
│   │   │   ├── FileDetailsDto.cs
│   │   │   ├── FileDto.cs
│   │   │   └── ImageCreateDto.cs
│   │   ├── Payment/
│   │   │   └── PaymentsDtos.cs
│   │   ├── Podcast/
│   │   │   └── PodcastEpisodeDtos.cs
│   │   ├── Programs/
│   │   │   ├── ExerciseAnswerDtos.cs
│   │   │   ├── LessonAttachmentDtos.cs
│   │   │   ├── LessonDtos.cs
│   │   │   ├── ProgramCertificateDtos.cs
│   │   │   ├── ProgramCourseDtos.cs
│   │   │   ├── ProgramEnrollmentDtos.cs
│   │   │   ├── ProgressDtos.cs
│   │   │   └── TrainingProgramDtos.cs
│   │   ├── StaticContent/
│   │   │   └── StaticContentCreateDto.cs
│   │   ├── Test/
│   │   │   ├── TestAttemptDtos.cs
│   │   │   ├── TestChoiceDtos.cs
│   │   │   ├── TestDtos.cs
│   │   │   └── TestQuestionDtos.cs
│   │   ├── Token/
│   │   │   └── TokenDtos.cs
│   │   ├── AccountDtos.cs
│   │   ├── ActivityLogDto.cs
│   │   ├── AttachmentDto.cs
│   │   ├── BaselmportDtos.cs
│   │   ├── ContentDtos.cs
│   │   ├── CultureDto.cs
│   │   ├── DomainDtos.cs
│   │   ├── ExamDtos.cs
│   │   ├── ExamResultDtos.cs
│   │   ├── FileCreateDtos.cs
│   │   ├── FileDetailsDtos.cs
│   │   ├── GoogleUserInfo.cs
│   │   ├── ImageCreateDto.cs
│   │   ├── IpDetailsDto.cs
│   │   ├── LinkDtos.cs
│   │   ├── MediaDetailsDto.cs
│   │   ├── NotificationDto.cs
│   │   ├── CityViaDto.cs
│   │   ├── ProgressDto.cs
│   │   ├── QualificationDtos.cs
│   │   ├── RefundDto.cs
│   │   ├── SmsTemplateDto.cs
│   │   ├── TaskDtos.cs
│   │   ├── TouristClubDto.cs
│   │   ├── TouristClubSubscriberDto.cs
│   │   └── UnsubscribeDtos.cs
│   ├── Elastic/
│   │   ├── Migrations/
│   │   │   └── 20230131211337_ReIndexDomain.cs
│   │   ├── ElasticDbContext.cs
│   │   ├── ElasticHelper.cs
│   │   └── ElasticMigration.cs
│   ├── Entities/
│   │   └── Tables/
│   │       ├── ActivityLog.cs
│   │       ├── BaseEntity.cs
│   │       ├── ChangeLog.cs
│   │       ├── ChangeLogTaskLog.cs
│   │       ├── Channel.cs
│   │       ├── ClubAmbassador.cs
│   │       ├── ClubPost.cs
│   │       ├── ClubPostLike.cs
│   │       ├── Contact.cs
│   │       ├── ContactEmailSchedule.cs
│   │       ├── CountryInfo.cs
│   │       ├── CourseLesson.cs
│   │       ├── Domain.cs
│   │       ├── EmailGroup.cs
│   │       ├── EmailLog.cs
│   │       ├── EmailSchedule.cs
│   │       ├── EmailTemplate.cs
│   │       ├── File.cs
│   │       ├── IpDetails.cs
│   │       ├── Job.cs
│   │       ├── JobApplication.cs
│   │       ├── LessonAttachment.cs
│   │       ├── LessonProgress.cs
│   │       ├── LessonSession.cs
│   │       ├── LiveCourse.cs
│   │       ├── LocalizationOption.cs
│   │       ├── LogRecord.cs
│   │       ├── MailServer.cs
│   │       ├── ParentalRating.cs
│   │       ├── Payment.cs
│   │       ├── PaymentItem.cs
│   │       ├── PodcastEpisode.cs
│   │       ├── ProgramCertificate.cs
│   │       ├── ProgramCourse.cs
│   │       ├── ProgramEnrollment.cs
│   │       ├── PromoCode.cs
│   │       ├── PromoCodeUsage.cs
│   │       ├── RefreshToken.cs
│   │       ├── Refund.cs
│   │       ├── SmsLog.cs
│   │       ├── SmsTemplate.cs
│   │       ├── StaticContent.cs
│   │       ├── TaskExecutionLog.cs
│   │       ├── Test.cs
│   │       ├── TestAnswer.cs
│   │       ├── TestAttempt.cs
│   │       ├── TestChoice.cs
│   │       ├── TestQuestion.cs
│   │       ├── TraineeProgress.cs
│   │       ├── TrainerInfo.cs
│   │       ├── TrainingProgram.cs
│   │       ├── Unsubscribe.cs
│   │       ├── User.cs
│   │       ├── UserLiveCourse.cs
│   │       ├── UserStatusHistory.cs
│   │       ├── WheelAward.cs
│   │       ├── WheelPlayer.cs
│   │       └── WheelPlayerState.cs
│   ├── Enums/
│   │   ├── AffiliateStatus.cs
│   │   ├── AwardType.cs
│   │   ├── CertificateDeliveryMethod.cs
│   │   ├── CertificateStatus.cs
│   │   ├── ClubPostStatus.cs
│   │   ├── CourseType.cs
│   │   ├── EnrollmentStatus.cs
│   │   ├── ErrorType.cs
│   │   ├── ExamParentEntityType.cs
│   │   ├── ExamStatus.cs
│   │   ├── FileValidationErrorType.cs
│   │   ├── JobApplicationStatus.cs
│   │   ├── JobType.cs
│   │   ├── LogEvents.cs
│   │   ├── MediaType.cs
│   │   ├── NotificationType.cs
│   │   ├── PaymentGatewayType.cs
│   │   ├── PaymentItemType.cs
│   │   ├── PaymentPurpose.cs
│   │   ├── PaymentStatus.cs
│   │   ├── PayoutStatus.cs
│   │   ├── ProcessStatus.cs
│   │   ├── RatingStatus.cs
│   │   ├── RefundStatus.cs
│   │   ├── SmsSendStatus.cs
│   │   ├── StaticContentMediaType.cs
│   │   ├── StaticContentType.cs
│   │   ├── VerificationMethod.cs
│   │   └── WorkplaceCategory.cs
│   ├── Exceptions/
│   │   ├── ChangeLogMigrationException.cs
│   │   ├── CustomException.cs
│   │   ├── EmailException.cs
│   │   ├── EntityNotFoundException.cs
│   │   ├── IdentityException.cs
│   │   ├── InvalidModelStateException.cs
│   │   ├── IpDetailsException.cs
│   │   ├── LocalizationException.cs
│   │   ├── MissingConfigurationException.cs
│   │   ├── NonPrimaryNodeException.cs
│   │   ├── NotFoundException.cs
│   │   ├── PluginDbContextException.cs
│   │   ├── PluginDbContextNotFoundException.cs
│   │   ├── PluginDbContextTooManyException.cs
│   │   ├── QueryException.cs
│   │   ├── ServerException.cs
│   │   ├── SynEmailLogTaskException.cs
│   │   ├── TaskNotFoundException.cs
│   │   ├── TaskRunnerDisabledException.cs
│   │   ├── TooManyRequestsException.cs
│   │   ├── UnauthorizedException.cs
│   │   └── ValidateModelStateAttribute.cs
│   ├── Extensions/
│   │   ├── ControllerExtensions.cs
│   │   ├── EntityValidationExtensions.cs
│   │   ├── GeneralResultExtensions.cs
│   │   ├── IFormFileExtensions.cs
│   │   ├── LoggerExtensions.cs
│   │   ├── PaginationExtensions.cs
│   │   └── ValidateModelExtensions.cs
│   ├── Formatters/
│   │   └── Csv/
│   │       ├── CsvClassMapHelper.cs
│   │       ├── CsvInputFormatter.cs
│   │       └── CsvOutputFormatter.cs
│   ├── Geography/
│   │   ├── Continent.cs
│   │   └── Country.cs
│   ├── Helpers/
│   │   ├── Customer/
│   │   │   └── UserHelper.cs
│   │   ├── DateTimeHelper.cs
│   │   ├── EnumHelper.cs
│   │   ├── FileNameHelper.cs
│   │   ├── FileValidatorHelper.cs
│   │   ├── GravatarHelper.cs
│   │   ├── HttpContextHelper.cs
│   │   ├── JsonHelper.cs
│   │   ├── StringHelper.cs
│   │   ├── TokenHelper.cs
│   │   ├── TypeHelper.cs
│   │   └── UidHelper.cs
│   ├── Identity/
│   │   ├── CustomSignInManager.cs
│   │   ├── GoogleCookieEventsHandler.cs
│   │   └── GoogleJwtBearerEventsHandler.cs
│   ├── Infrastructure/
│   │   ├── PermissionInfra/
│   │   │   ├── Permissions.cs
│   │   │   └── PermissionsSeeding.cs
│   │   ├── StaticContentInfra/
│   │   │   ├── StaticContentDefaults.cs
│   │   │   ├── StaticContentKeys.cs
│   │   │   └── StaticContentSeeder.cs
│   │   ├── AppRoles.cs
│   │   ├── CustomSqlServerMigrationsSqlGenerator.cs
│   │   ├── CustomSwaggerScheme.cs
│   │   ├── DbContextExtensions.cs
│   │   ├── DBQueryProvider.cs
│   │   ├── DictionaryExtensions.cs
│   │   ├── ESOnlyQueryProviderFactory.cs
│   │   ├── ESQueryProvider.cs
│   │   ├── IdentityHelper.cs
│   │   ├── LockManager.cs
│   │   ├── MixedQueryProvider.cs
│   │   ├── PluginLoadContext.cs
│   │   ├── PluginManager.cs
│   │   ├── QueryCommand.cs
│   │   ├── QueryModelBuilder.cs
│   │   ├── QueryProviderFactory.cs
│   │   ├── QueryStringParser.cs
│   │   ├── ResponseHeaderNames.cs
│   │   └── TaskRunner.cs
│   ├── Interfaces/
│   │   ├── AffiliateMarketingIntf/
│   │   │   └── IAffiliateServices.cs
│   │   ├── AuthenticationIntf/
│   │   │   └── IAuthenticationServices.cs
│   │   ├── AuthorizationIntf/
│   │   │   ├── IPermissionServices.cs
│   │   │   └── IRoleServices.cs
│   │   ├── ClubIntf/
│   │   │   ├── IAmbassadorServices.cs
│   │   │   ├── IPostLikeServices.cs
│   │   │   ├── IPostService.cs
│   │   │   ├── IWheelAwardServices.cs
│   │   │   └── IWheelPlayerServices.cs
│   │   ├── EmailIntf/
│   │   │   ├── IEmailConfirmation.cs
│   │   │   └── IEmailSend.cs
│   │   ├── JobIntf/
│   │   │   └── IJobServices.cs
│   │   ├── LiveCourseIntf/
│   │   │   └── ILiveCourseServices.cs
│   │   ├── PaymentIntf/
│   │   │   ├── IPaymentGatewayAdapters.cs
│   │   │   ├── IPaymentRepository.cs
│   │   │   ├── IPaymentService.cs
│   │   │   ├── IPaymentVerifier.cs
│   │   │   └── IRefundRepository.cs
│   │   ├── PodcastEpisodeIntf/
│   │   │   └── IPodcastEpisodeService.cs
│   │   ├── ProgramIntf/
│   │   │   ├── ICertificateServices.cs
│   │   │   ├── ICourseLessonServices.cs
│   │   │   ├── IEnrollmentServices.cs
│   │   │   ├── ILessonAttachmentServices.cs
│   │   │   ├── IProgramCourseServices.cs
│   │   │   ├── IProgressServices.cs
│   │   │   └── ITrainingProgramServices.cs
│   │   ├── QueriesIntf/
│   │   │   └── IQueryServices.cs
│   │   ├── StaticContentIntf/
│   │   │   └── IStaticContentServices.cs
│   │   ├── TestIntf/
│   │   │   ├── ITestAttemptServices.cs
│   │   │   ├── ITestChoiceServices.cs
│   │   │   ├── ITestQuestionServices.cs
│   │   │   └── ITestServices.cs
│   │   ├── TokenIntf/
│   │   │   └── ITokenServices.cs
│   │   ├── UserIntf/
│   │   │   ├── IUserRepository.cs
│   │   │   └── IUserServices.cs
│   │   ├── IAccountExternalService.cs
│   │   ├── IBaseEntity.cs
│   │   ├── IBaseService.cs
│   │   ├── IBaseServiceWithoutUpdate.cs
│   │   ├── ICacheService.cs
│   │   ├── ICascadeDeleteService.cs
│   │   ├── IContactService.cs
│   │   ├── IDomainService.cs
│   │   ├── IEmailFromTemplateService.cs
│   │   ├── IEmailSchedulingService.cs
│   │   ├── IEmailService.cs
│   │   ├── IEmailValidationExternalService.cs
│   │   ├── IEmailVerificationExtension.cs
│   │   ├── IEmailVerificationService.cs
│   │   ├── IEmailVerifyService.cs
│   │   ├── IEmailWithLogService.cs
│   │   ├── IEntityService.cs
│   │   ├── IExtendedBaseService.cs
│   │   ├── IExternalAuthProvider.cs
│   │   ├── IExternalAuthService.cs
│   │   ├── IHttpContextHelper.cs
│   │   ├── IIdentityService.cs
│   │   ├── IImportService.cs
│   │   ├── ILocalizationManager.cs
│   │   ├── ILockService.cs
│   │   ├── IMxVerifyService.cs
│   │   ├── IPlugin.cs
│   │   ├── IQueryProvider.cs
│   │   ├── IRefundServices.cs
│   │   ├── IServerConfigurationManager.cs
│   │   ├── ISharedData.cs
│   │   ├── IStudentProgress.cs
│   │   ├── ISwaggerConfigurator.cs
│   │   ├── ITask.cs
│   │   ├── IVariablesProvider.cs
│   │   └── IVariablesService.cs
│   ├── Localization/
│   │   ├── Ratings/
│   │   ├── countries.json
│   │   ├── iso6392.txt
│   │   └── (other translate file)
│   ├── Middlewares/
│   │   ├── CultureMiddleware.cs
│   │   └── PermissionMiddleware.cs
│   ├── obj/
│   ├── Services/
│   │   ├── AffiliateMarketingSvc/
│   │   │   └── AffiliateService.cs
│   │   ├── AuthenticationSvc/
│   │   │   └── AuthenticationService.cs
│   │   ├── AuthorizationSvc/
│   │   │   ├── PermissionService.cs
│   │   │   └── RoleService.cs
│   │   ├── BaseSvc/
│   │   │   ├── CascadeDeleteService.cs
│   │   │   └── ExtendedBaseService.cs
│   │   ├── ClubSvc/
│   │   │   ├── AmbassadorService.cs
│   │   │   ├── PostLikeService.cs
│   │   │   ├── PostService.cs
│   │   │   ├── WheelAwardService.cs
│   │   │   └── WheelPlayerService.cs
│   │   ├── Core/
│   │   │   ├── Langs/
│   │   │   │   ├── ar.json
│   │   │   │   └── en-US.json
│   │   │   └── Messages/
│   │   │       ├── AffiliateMessage.cs
│   │   │       ├── AuthenticationMessage.cs
│   │   │       ├── CertificateMessages.cs
│   │   │       ├── ClubMessage.cs
│   │   │       ├── CourseLessonMessages.cs
│   │   │       ├── CourseMessage.cs
│   │   │       ├── EnrollmentMessage.cs
│   │   │       ├── GeneralMessage.cs
│   │   │       ├── JobMessages.cs
│   │   │       ├── LessonAttachmentMessage.cs
│   │   │       ├── LiveCourseMessage.cs
│   │   │       ├── PaymentMessage.cs
│   │   │       ├── PermissionMessage.cs
│   │   │       ├── PodcastEpisodeMessage.cs
│   │   │       ├── ProgressMessage.cs
│   │   │       ├── RoleMessages.cs
│   │   │       ├── TestMessage.cs
│   │   │       ├── TrackingMessage.cs
│   │   │       ├── TrainingProgramMessage.cs
│   │   │       └── WheelMessage.cs
│   │   ├── EmailSvc/
│   │   │   ├── EmailConfirmationService.cs
│   │   │   └── EmailSendService.cs
│   │   ├── JobServiceSvc/
│   │   │   └── JobService.cs
│   │   ├── LiveCourseSvc/
│   │   │   └── LiveCourseService.cs
│   │   ├── OTPSvc/
│   │   │   └── OtpService.cs
│   │   ├── PaymentSvc/
│   │   │   ├── Gateways/
│   │   │   │   ├── MockGatewayAdapter.cs
│   │   │   │   └── PayTabsAdapter.cs
│   │   │   ├── PaymentRepository.cs
│   │   │   ├── PaymentService.cs
│   │   │   ├── PaymentVerifierService.cs
│   │   │   └── RefundRepository.cs
│   │   ├── PodcastEpisodeSvc/
│   │   │   └── PodcastEpisodeService.cs
│   │   ├── ProgramsSvc/
│   │   │   ├── CertificateService.cs
│   │   │   ├── CourseLessonService.cs
│   │   │   ├── EnrollmentService.cs
│   │   │   ├── LessonAttachmentService.cs
│   │   │   ├── ProgramCourseService.cs
│   │   │   ├── ProgressService.cs
│   │   │   └── TrainingProgramService.cs
│   │   ├── QueriesSvc/
│   │   │   └── QueryService.cs
│   │   ├── StaticContentSvc/
│   │   │   └── StaticContentService.cs
│   │   ├── TestSvc/
│   │   │   ├── TestAttemptService.cs
│   │   │   ├── TestChoiceService.cs
│   │   │   ├── TestQuestionService.cs
│   │   │   └── TestService.cs
│   │   ├── TokenSvc/
│   │   │   └── TokenService.cs
│   │   ├── UserSvc/
│   │   │   ├── UserRepository.cs
│   │   │   └── UserService.cs
│   │   ├── AccountExternalService.cs
│   │   ├── ActivityLogService.cs
│   │   ├── ContactService.cs
│   │   ├── DomainService.cs
│   │   ├── EmailFromTemplateService.cs
│   │   ├── EmailSchedulingService.cs
│   │   ├── EmailService.cs
│   │   ├── EmailValidationExternalService.cs
│   │   ├── EmailVerificationExtensionService.cs
│   │   ├── EmailVerificationService.cs
│   │   ├── EmailVerifyService.cs
│   │   ├── EmailWithLogService.cs
│   │   ├── GoogleAuthService.cs
│   │   ├── IdentityService.cs
│   │   ├── ImportService.cs
│   │   ├── IpDetailsService.cs
│   │   ├── LocalizationManager.cs
│   │   ├── LockService.cs
│   │   ├── MemoryCacheService.cs
│   │   ├── MxVerifyService.cs
│   │   ├── ServerConfigurationManager.cs
│   │   ├── TaskStatusService.cs
│   │   ├── ValidationRegistration.cs
│   │   └── VariablesService.cs
│   ├── Tasks/
│   │   ├── BaseTask.cs
│   │   ├── ChangeLogTask.cs
│   │   ├── ContactScheduledEmailTask.cs
│   │   ├── DomainVerificationTask.cs
│   │   ├── HardDeleteTask.cs
│   │   ├── SyncEmailLogTask.cs
│   │   ├── SyncEsTask.cs
│   │   ├── SyncIpDetailsTask.cs
│   │   └── SyncProgramProgressTask.cs
│   ├── stylecop.json
│   └── Usings.cs
└── Lumora.Web/
├── Connected Services/
├── Dependencies/
├── Properties/
│   └── PublishProfiles/
│       ├── FolderProfile.pubxml
│       └── launchSettings.json
├── bin/
├── Controllers/
│   ├── AffiliateMarketingAPI/
│   │   └── AffiliateController.cs
│   ├── AuthenticationAPI/
│   │   ├── Admin/
│   │   │   └── UsersAdminController.cs
│   │   ├── AccountManagementController.cs
│   │   ├── AuthenticatedController.cs
│   │   ├── AuthenticationController.cs
│   │   ├── ConfirmationController.cs
│   │   ├── PasswordManagementController.cs
│   │   └── TwoFAManagementController.cs
│   ├── AuthorizationAPI/
│   │   ├── PermissionsManagementController.cs
│   │   └── RolesManagementController.cs
│   ├── ClubAPI/
│   │   └── Admin/
│   │       └── AdministController.cs
│   ├── EmailAPI/
│   ├── JobAPI/
│   │   ├── Admin/
│   │   │   └── AdministController.cs
│   │   └── JobController.cs
│   ├── LiveCourseAPI/
│   │   ├── Admin/
│   │   │   └── LiveCourseAdminController.cs
│   │   └── LiveCourseController.cs
│   ├── LocalizationAPI/
│   │   └── LocalizationController.cs
│   ├── PaymentAPI/
│   │   └── PaymentController.cs
│   ├── PodcastEpisodeAPI/
│   │   ├── Admin/
│   │   │   └── PodcastEpisodeAdminController.cs
│   │   └── PodcastEpisodeController.cs
│   ├── ProgramsAPI/
│   │   ├── Admin/
│   │   │   ├── CertificateAdminController.cs
│   │   │   ├── LessonAdminController.cs
│   │   │   ├── LessonAttachmentAdminController.cs
│   │   │   ├── ProgramCourseAdminController.cs
│   │   │   ├── ProgramEnrollmentAdminController.cs
│   │   │   └── ProgramManagementAdminController.cs
│   │   ├── CertificateController.cs
│   │   ├── LessonAttachmentController.cs
│   │   ├── LessonController.cs
│   │   ├── ProgramCourseController.cs
│   │   ├── ProgramEnrollmentController.cs
│   │   ├── ProgramsManagementController.cs
│   │   └── ProgressController.cs
│   ├── StaticContentAPI/
│   │   └── StaticContentController.cs
│   ├── TestAPI/
│   │   ├── Admin/
│   │   │   ├── TestAdminController.cs
│   │   │   ├── TestChoiceAdminController.cs
│   │   │   └── TestQuestionAdminController.cs
│   │   └── TestAttemptController.cs
│   ├── BaseController.cs
│   ├── ErrorsController.cs
│   ├── GeographyController.cs
│   ├── GoogleController.cs
│   ├── ImportController.cs
│   ├── LocksController.cs
│   ├── LogsController.cs
│   └── TasksController.cs
├── Extensions/
│   ├── AccountExtensions.cs
│   ├── ApiSettingsExtensions.cs
│   ├── CacheProfileExtensions.cs
│   ├── ControllerExtensions.cs
│   ├── ConsoleExtensions.cs
│   ├── EmailExtensions.cs
│   ├── IdentityExtensions.cs
│   ├── InfrastructureExtensions.cs
│   ├── IpDetailsExtensions.cs
│   ├── LoggingExtensions.cs
│   ├── MediaExtensions.cs
│   ├── MigrationExtensions.cs
│   ├── QuartzExtensions.cs
│   ├── RequestLimitExtensions.cs
│   ├── ServiceRegistrationExtensions.cs
│   ├── SwaggerExtensions.cs
│   ├── TaskExtensions.cs
│   └── ValidationExtensions.cs
├── Filters/
│   └── SwaggerEntitiesFilter.cs
├── Migrations/
│   └── PGDContextModelSnapshot.cs
├── Resources/
│   ├── disposable_domains.txt
│   └── free_domains.txt
├── Validations/
│   ├── AuthenticationVal/
│   │   ├── ChangePasswordValidator.cs
│   │   ├── ChangePhoneNumberValidator.cs
│   │   ├── CompleteUserDataValidator.cs
│   │   ├── ConfirmEmailValidator.cs
│   │   ├── Enable2FAValidator.cs
│   │   ├── ForgotPasswordValidator.cs
│   │   ├── Login2FAValidator.cs
│   │   ├── LoginValidator.cs
│   │   ├── RegisterValidator.cs
│   │   ├── ResetPasswordValidator.cs
│   │   └── UserUpdateDtoValidator.cs
│   ├── AuthorizationVal/
│   │   ├── AddMultiplePermissionsValidator.cs
│   │   ├── AddPermissionValidator.cs
│   │   └── RemovePermissionValidator.cs
│   ├── BaseVal/
│   │   └── PaginationValidator.cs
│   ├── ClubVal/
│   │   ├── AmbassadorAssignValidator.cs
│   │   ├── PostCreateValidator.cs
│   │   └── PostStatusUpdateValidator.cs
│   ├── Customer/
│   │   └── UserValidator.cs
│   ├── JobVal/
│   │   ├── JobApplicationCreateValidator.cs
│   │   ├── JobCreateValidator.cs
│   │   └── JobUpdateValidator.cs
│   ├── LiveCourseValidators/
│   │   ├── LiveCourseCreateValidator.cs
│   │   └── LiveCourseUpdateValidator.cs
│   ├── PodcastEpisodeVal/
│   │   ├── PodcastEpisodeCreateValidator.cs
│   │   └── PodcastEpisodeUpdateValidator.cs
│   ├── ProgramVal/
│   │   ├── CourseLessonAttachmentCreateValidator.cs
│   │   ├── CourseLessonAttachmentUpdateValidator.cs
│   │   ├── CourseUpdateValidator.cs
│   │   ├── CourseWithLessonsCreateValidator.cs
│   │   ├── LessonAttachmentCreateValidator.cs
│   │   ├── LessonAttachmentUpdateValidator.cs
│   │   ├── LessonCreateValidator.cs
│   │   ├── LessonReorderValidator.cs
│   │   ├── LessonsWithContentCreateValidator.cs
│   │   ├── LessonUpdateValidator.cs
│   │   ├── ProgramCourseUpdateValidator.cs
│   │   ├── SingleLessonAttachmentCreateValidator.cs
│   │   ├── SubmitAnswerValidator.cs
│   │   ├── TrainingProgramCreateValidator.cs
│   │   ├── TrainingProgramUpdateValidator.cs
│   │   └── TrainingProgressCreateValidator.cs
│   ├── TestVal/
│   │   ├── RelatedTestChoiceCreateValidator.cs
│   │   ├── RelatedTestChoiceUpdateValidator.cs
│   │   ├── RelatedTestQuestionCreateValidator.cs
│   │   ├── RelatedTestQuestionUpdateValidator.cs
│   │   ├── TestCreateValidator.cs
│   │   └── TestUpdateValidator.cs
│   ├── GenericValidator.cs
│   ├── NotificationValidator.cs
│   ├── QualificationValidator.cs
│   ├── RefundValidator.cs
│   ├── TouristClubSubscriberValidator.cs
│   └── TouristClubValidator.cs
├── appsettings.Development.json
├── appsettings.json
├── Dockerfile
├── Program.cs
├── stylecop.json
├── Usings.cs
└── Wejha.targets

### Prerequisites
.NET 8 SDK or later
PostgreSQL
Package managers: NuGet

###Install dependencies:
dotnet restore
Set up the database:
dotnet ef database update
Run the project:
dotnet run

###Development
Service Separation: Each module (Subscription, Course, Payment) is independent for maintainability.
Custom DTOs per Module: Simplifies data transfer between layers.
MemoryCache / Caching: Improves performance for content retrieval.
Localization: Supports both English and Arabic messages.
Logging & Error Handling: Professional logging for easy issue tracking.

###Security
Role-based user and permission management.
Sensitive data protection via environment files (User Secrets / .env).
Secure API endpoints using JWT Tokens or Identity Server.

###Future Expansion
Add additional languages.
Integrate AI/Chatbot to assist users.
Expand the subscription and digital products system.

###Contributors
Lead Developer: Abdulmoneim Omar Alward
Future contributors can be added following professional standards.

###License
This project is personally owned by the developer and can be reused for personal or commercial purposes while respecting author rights.
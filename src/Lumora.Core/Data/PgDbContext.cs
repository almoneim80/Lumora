using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;
using Job = Lumora.Entities.Tables.Job;

namespace Lumora.Data
{
    /// <summary>
    /// PgDbContext Class.
    /// </summary>
    public partial class PgDbContext : IdentityDbContext<User>
    {
        public readonly IConfiguration Configuration;
        private readonly IHttpContextHelper? httpContextHelper;
        protected PgDbContext()
        {
            try
            {
                Console.WriteLine("Initializing PgDbContext...");

                Configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", false)
                    .AddEnvironmentVariables()
                   .AddUserSecrets(typeof(PgDbContext).Assembly)
                    .Build();

                Console.WriteLine("PgDbContext initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create PgDbContext. Error: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                throw;
            }
        }
        public PgDbContext(DbContextOptions<PgDbContext> options, IConfiguration configuration, IHttpContextHelper httpContextHelper)
            : base(options)
        {
            Configuration = configuration;
            this.httpContextHelper = httpContextHelper;
        }
        public bool IsImportRequest { get; set; }

        // ************** Tables **************
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public virtual DbSet<UserStatusHistory> UserStatusHistories { get; set; } = null!;
        public virtual DbSet<Unsubscribe> Unsubscribes { get; set; } = null!;
        public virtual DbSet<IpDetails> IpDetails { get; set; } = null!;
        public virtual DbSet<Contact> Contacts { get; set; } = null!;
        public virtual DbSet<Domain> Domains { get; set; } = null!;
        public virtual DbSet<MailServer> MailServers { get; set; } = null!;
        public DbSet<SmsTemplate> SmsTemplates { get; set; } = null!;

        // ************ End Tables *******************

        /// <summary>
        /// Save changes.
        /// </summary>
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var result = 0;
            var changes = new Dictionary<EntityEntry, ChangeLog>();

            var entries = ChangeTracker
               .Entries()
               .Where(e => e.Entity is BaseEntityWithId && (
                       e.State == EntityState.Added
                       || e.State == EntityState.Modified
                       || e.State == EntityState.Deleted));

            var currentUserId = await httpContextHelper!.GetCurrentUserIdAsync();
            var userIpAddress = httpContextHelper!.IpAddressV4;
            var userAgent = httpContextHelper!.UserAgent;

            if (entries.Any())
            {
                foreach (var entityEntry in entries)
                {
                    if (entityEntry.State == EntityState.Added)
                    {
                        var createdAtEntity = entityEntry.Entity as IHasCreatedAt;

                        if (createdAtEntity is not null)
                        {
                            createdAtEntity.CreatedAt = createdAtEntity.CreatedAt == DateTimeOffset.MinValue
                                ? DateTimeOffset.UtcNow
                                : createdAtEntity.CreatedAt.ToUniversalTime();
                        }

                        var createdByEntity = entityEntry.Entity as IHasCreatedBy;

                        if (createdByEntity is not null)
                        {
                            createdByEntity.CreatedById = currentUserId;
                            createdByEntity.CreatedByIp = string.IsNullOrEmpty(createdByEntity.CreatedByIp) ? userIpAddress : createdByEntity.CreatedByIp;
                            createdByEntity.CreatedByUserAgent = string.IsNullOrEmpty(createdByEntity.CreatedByUserAgent) ? userAgent : createdByEntity.CreatedByUserAgent;
                        }
                    }

                    if (entityEntry.State == EntityState.Modified)
                    {
                        var updatedAtEntity = entityEntry.Entity as IHasUpdatedAt;

                        if (updatedAtEntity is not null)
                        {
                            updatedAtEntity.UpdatedAt = IsImportRequest && updatedAtEntity.UpdatedAt.HasValue
                                ? updatedAtEntity.UpdatedAt.Value.ToUniversalTime()
                                : DateTime.UtcNow;
                        }

                        var updatedByEntity = entityEntry.Entity as IHasUpdatedBy;

                        if (updatedByEntity is not null)
                        {
                            updatedByEntity.UpdatedById = currentUserId;
                            updatedByEntity.UpdatedByIp = IsImportRequest && !string.IsNullOrEmpty(updatedByEntity.UpdatedByIp) ? updatedByEntity.UpdatedByIp : userIpAddress;
                            updatedByEntity.UpdatedByUserAgent = IsImportRequest && !string.IsNullOrEmpty(updatedByEntity.UpdatedByUserAgent) ? updatedByEntity.UpdatedByUserAgent : userAgent;
                        }
                    }

                    var entityType = entityEntry.Entity.GetType();

                    if (entityType!.GetCustomAttributes<SupportsChangeLogAttribute>().Any())
                    {
                        // save entity state as it is before SaveChanges call
                        changes[entityEntry] = new ChangeLog
                        {
                            ObjectType = entityEntry.Entity.GetType().Name,
                            EntityState = entityEntry.State,
                            CreatedAt = DateTime.UtcNow,
                        };
                    }
                }
            }

            if (changes.Count > 0)
            {
                // save original records and obtain ids (to preserve ids in change_log)
                result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

                foreach (var change in changes)
                {
                    // save object id which we only recieve after SaveChanges (for new records)
                    change.Value.ObjectId = ((BaseEntityWithId)change.Key.Entity).Id;
                    change.Value.Data = JsonHelper.Serialize(change.Key.Entity);
                }

                ChangeLogs!.AddRange(changes.Values);
            }

            return result + await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// Configuring PgDbContext.
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                Console.WriteLine("Configuring PgDbContext...");

                var postgresConfig = Configuration.GetSection("Postgres").Get<PostgresConfig>();

                if (postgresConfig == null)
                {
                    throw new MissingConfigurationException("Postgres configuration is mandatory.");
                }

                optionsBuilder.UseNpgsql(
                    postgresConfig.ConnectionString,
                    b => b.MigrationsHistoryTable("_migrations")
                          .MigrationsAssembly("Lumora.Web"))
                    .UseSnakeCaseNamingConvention()
                    .ReplaceService<IMigrationsSqlGenerator, CustomSqlServerMigrationsSqlGenerator>();

                Console.WriteLine("PgDbContext successfully configured");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to configure PgDbContext. Error: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Configuring PgDbContext.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // all deleted records should be hidden
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(ISharedData).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var prop = Expression.Property(parameter, nameof(ISharedData.IsDeleted));
                    var filter = Expression.Lambda(
                        Expression.Equal(prop, Expression.Constant(false)),
                        parameter);
                    builder.Entity(entityType.ClrType).HasQueryFilter(filter);
                }
            }

            // Override default AspNet Identity table names
            builder.Entity<User>(entity => { entity.ToTable(name: "users"); });
            builder.Entity<IdentityRole>(entity => { entity.ToTable(name: "roles"); });
            builder.Entity<IdentityUserRole<string>>(entity => { entity.ToTable("user_roles"); });
            builder.Entity<IdentityUserClaim<string>>(entity => { entity.ToTable("user_claims"); });
            builder.Entity<IdentityUserLogin<string>>(entity => { entity.ToTable("user_logins"); });
            builder.Entity<IdentityUserToken<string>>(entity => { entity.ToTable("user_tokens"); });
            builder.Entity<IdentityRoleClaim<string>>(entity => { entity.ToTable("role_claims"); });

            // ************ Relationships ************
            
            // LessonAttachment relationships
            builder.Entity<LessonAttachment>()
                .HasOne(la => la.CourseLesson)
                .WithMany(l => l.LessonAttachments)
                .HasForeignKey(la => la.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

            // The relationship between StudentProgress and User
            builder.Entity<TraineeProgress>()
                .HasOne(sp => sp.User)
                .WithMany(u => u.StudentProgresses)
                .HasForeignKey(sp => sp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProgramCourse>()
                .HasOne(c => c.TrainingProgram)
                .WithMany(p => p.ProgramCourses)
                .HasForeignKey(c => c.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TestQuestion>()
                .HasOne(e => e.Test)
                .WithMany(l => l.Questions)
                .HasForeignKey(e => e.TestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TestChoice>()
                .HasOne(ec => ec.TestQuestion)
                .WithMany(e => e.Choices)
                .HasForeignKey(ec => ec.TestQuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Job>()
                .Property(j => j.WorkplaceCategory)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Entity<Job>()
                .Property(s => s.JobType)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Entity<JobApplication>()
                .Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Entity<ProgramEnrollment>()
                .HasOne(c => c.TrainingProgram)
                .WithMany(p => p.ProgramEnrollments)
                .HasForeignKey(c => c.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProgramEnrollment>()
                .HasOne(e => e.User)
                .WithMany(u => u.ProgramEnrollments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProgramEnrollment>()
                .Property(s => s.EnrollmentStatus)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Entity<ClubPost>()
                .Property(s => s.MediaType)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Entity<ClubPost>()
                .Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Entity<CourseLesson>()
                .HasOne(cl => cl.LessonTest)
                .WithOne(t => t.CourseLesson)
                .HasForeignKey<Test>(t => t.LessonId);

            builder.Entity<TestQuestion>()
                .Property(q => q.DisplayOrder)
                .HasDefaultValue(0);

            builder.Entity<TestChoice>()
                .Property(c => c.DisplayOrder)
                .HasDefaultValue(0);

            builder.Entity<TestQuestion>()
                .HasIndex(q => new { q.TestId, q.DisplayOrder })
                .IsUnique();

            builder.Entity<TestChoice>()
                .HasIndex(c => new { c.TestQuestionId, c.DisplayOrder })
                .IsUnique();

            builder.Entity<TestAnswer>()
                .HasOne(a => a.TestChoice)
                .WithMany(c => c.Answers)
                .HasForeignKey(a => a.SelectedChoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProgramEnrollment>()
                .HasOne(e => e.Certificate)
                .WithOne(c => c.ProgramEnrollment)
                .HasForeignKey<ProgramCertificate>(c => c.EnrollmentId);

            var stringListConverter = new ValueConverter<List<string>?, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v!, (JsonSerializerOptions?)null) ?? new List<string>());

            builder.Entity<TrainingProgram>(entity =>
            {
                entity.Property(p => p.Audience).HasConversion(stringListConverter).HasColumnType("text");
                entity.Property(p => p.Requirements).HasConversion(stringListConverter).HasColumnType("text");
                entity.Property(p => p.Topics).HasConversion(stringListConverter).HasColumnType("text");
                entity.Property(p => p.Goals).HasConversion(stringListConverter).HasColumnType("text");
                entity.Property(p => p.Outcomes).HasConversion(stringListConverter).HasColumnType("text");

                entity.Property(p => p.Trainers)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<TrainerInfo>>(v, (JsonSerializerOptions?)null) ?? new())
                    .HasColumnType("text");
            });
        }
    }
}

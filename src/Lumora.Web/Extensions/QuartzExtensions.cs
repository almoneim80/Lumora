namespace Lumora.Web.Extensions
{
    public static class QuartzExtensions
    {
        // Configure Quartz
        public static void ConfigureQuartz(this WebApplicationBuilder builder)
        {
            var taskRunnerSchedule = builder.Configuration.GetValue<string>("TaskRunner:CronSchedule")!;

            builder.Services.AddQuartz(q =>
            {
                q.AddJob<TaskRunner>(opts => opts.WithIdentity("TaskRunner"));
                q.AddTrigger(opts =>
                opts.ForJob("TaskRunner").WithIdentity("TaskRunnerTrigger").WithCronSchedule(taskRunnerSchedule));
            });

            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            builder.Services.AddTransient<TaskRunner>();
        }
    }
}

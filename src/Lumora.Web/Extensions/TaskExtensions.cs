namespace Lumora.Web.Extensions
{
    public static class TaskExtensions
    {
        // configure tasks
        public static void ConfigureTasks(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<ITask, SyncEsTask>();
            builder.Services.AddScoped<ITask, SyncIpDetailsTask>();
            builder.Services.AddScoped<ITask, DomainVerificationTask>();
            builder.Services.AddScoped<ITask, ContactScheduledEmailTask>();
            builder.Services.AddScoped<ITask, SyncEmailLogTask>();
            //builder.Services.AddScoped<ITask, HardDeleteTask>();
        }
    }
}

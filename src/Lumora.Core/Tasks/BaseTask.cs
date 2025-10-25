﻿using Lumora.Configuration;

namespace Lumora.Tasks;

public abstract class BaseTask : ITask
{
    protected readonly string configKey;

    private readonly TaskStatusService taskStatusService;

    protected BaseTask(string configKey, IConfiguration configuration, TaskStatusService taskStatusService)
    {
        this.configKey = configKey;
        this.taskStatusService = taskStatusService;

        var config = configuration.GetSection(configKey)!.Get<TaskConfig>();

        if (config is not null)
        {
            CronSchedule = config.CronSchedule;
            RetryCount = config.RetryCount;
            RetryInterval = config.RetryInterval;

            taskStatusService.SetInitialState(Name, config.Enable);
        }
        else
        {
            throw new MissingConfigurationException($"The specified configuration section for the provided configKey {configKey} could not be found in the settings file.");
        }
    }

    public string Name
    {
        get
        {
            return GetType().Name;
        }
    }

    public string CronSchedule { get; private set; }

    public int RetryCount { get; private set; }

    public int RetryInterval { get; private set; }

    public bool IsRunning
    {
        get
        {
            return taskStatusService.IsRunning(Name);
        }
    }

    public void SetRunning(bool running)
    {
        taskStatusService.SetRunning(Name, running);
    }

    public abstract Task<bool> Execute(TaskExecutionLog currentJob);
}

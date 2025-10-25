//using System.Linq;
//using Microsoft.Extensions.Configuration;

//namespace Lumora.Controllers;

//[Authorize(Roles = "Admin")]
//[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
//public class TasksController : ControllerBase
//{
//    private readonly IEnumerable<ITask> tasks;

//    private readonly TaskRunner taskRunner;

//    public TasksController(IEnumerable<ITask> tasks, TaskRunner taskRunner, IConfiguration configuration)
//    {
//        this.taskRunner = taskRunner;
//        this.tasks = tasks;
//    }

//    /// <summary>
//    /// Get all tasks.
//    /// </summary>
//    [HttpGet]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public ActionResult Get()
//    {
//        return Ok(tasks.Select(t => CreateTaskDetailsDto(t)));
//    }

//    /// <summary>
//    /// Get task details.
//    /// </summary>
//    [HttpGet("{name}")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public TaskDetailsDto Get(string name)
//    {
//        var result = tasks.Where(t => t.Name == name);

//        if (!result.Any())
//        {
//            throw new TaskNotFoundException(name);
//        }
//        else
//        {
//            return CreateTaskDetailsDto(result.First());
//        }
//    }

//    /// <summary>
//    /// Start task.
//    /// </summary>
//    [HttpGet("start/{name}")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public TaskDetailsDto Start(string name)
//    {
//        return StartOrStop(name, true);
//    }

//    /// <summary>
//    /// Stop task.
//    /// </summary>
//    [HttpGet("stop/{name}")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public TaskDetailsDto Stop(string name)
//    {
//        return StartOrStop(name, false);
//    }

//    /// <summary>
//    /// Execute task.
//    /// </summary>
//    [HttpGet("execute/{name}")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
//    public async Task<TaskExecutionDto> Execute(string name)
//    {
//        var result = tasks.Where(t => t.Name == name);

//        if (!result.Any())
//        {
//            throw new TaskNotFoundException(name);
//        }
//        else
//        {
//            var completed = await taskRunner.ExecuteTask(result.First());
//            return new TaskExecutionDto
//            {
//                Name = name,
//                Completed = completed,
//            };
//        }
//    }

//    /// <summary>
//    /// Start or stop task.
//    /// </summary>
//    private TaskDetailsDto StartOrStop(string name, bool start)
//    {
//        var result = tasks.Where(t => t.Name == name);

//        if (!result.Any())
//        {
//            throw new TaskNotFoundException(name);
//        }
//        else
//        {
//            var task = result.First();
//            taskRunner.StartOrStopTask(task, start);
//            return CreateTaskDetailsDto(task);
//        }
//    }

//    /// <summary>
//    /// Create task details dto.
//    /// </summary>
//    private TaskDetailsDto CreateTaskDetailsDto(ITask task)
//    {
//        return new TaskDetailsDto
//        {
//            Name = task.Name,
//            CronSchedule = task.CronSchedule,
//            RetryCount = task.RetryCount,
//            RetryInterval = task.RetryInterval,
//            IsRunning = task.IsRunning,
//        };
//    }
//}

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

public class HardwareController
{
    private readonly Action<string> _log;
    private readonly Subject<Command> _commandSubject = new();
    private bool _isRunningCommand = false;

    public HardwareController(Action<string> log)
    {
        _log = log;
        _commandSubject
            .Throttle(TimeSpan.FromMilliseconds(100))
            .Subscribe(async cmd =>
            {
                _isRunningCommand = true;

                _log($"\t - {cmd.Param.Id}:Running {cmd.Name} command ({cmd.Param.Id}) with params: {cmd.Param}");

                try
                {
                    await cmd.Task();

                    _log($"\t - {cmd.Param.Id}:Done :)");
                    cmd.TaskCompletionSource.SetResult();
                }
                catch (Exception ex)
                {
                    cmd.TaskCompletionSource.SetException(ex);
                }
                finally
                {
                    _isRunningCommand = false;
                }
            });
    }

    public Task RunCommand(string command, CommandParam param)
    {
        if (_isRunningCommand)
        {
            _log($"\t - {param.Id}: Ignoring {command} command with params: {param}");
            return Task.CompletedTask;
        }

        var cmd = new Command
        {
            Name = command,
            Param = param,
            TaskCompletionSource = new TaskCompletionSource(),
            Task = command switch
            {
                "move" => () => Move((MoveCommandParam)param),
                "stop" => () => Stop(param),
                _ => throw new ArgumentOutOfRangeException(nameof(command)),
            }
        };

        _commandSubject.OnNext(cmd);

        return cmd.TaskCompletionSource.Task;
    }

    private async Task Move(MoveCommandParam param)
    {
        var dice = Random.Shared.Next(3);
        if (dice == 0)
        {
            throw new InvalidOperationException("You're out of luck!");
        }

        await Task.Delay(1000);
    }

    private async Task Stop(CommandParam param)
    {
        await Task.Delay(500);
    }
}

public class Command
{
    public string Name { get; set; }
    public Func<Task> Task { get; set; }
    public CommandParam Param { get; set; }
    public TaskCompletionSource TaskCompletionSource { get; set; }
}

public class CommandParam
{
    public int Id { get; set; }
    public override string ToString()
       => $"Id: {Id}";
}

public class MoveCommandParam : CommandParam
{
    public int Steps { get; set; }

    public override string ToString()
        => base.ToString() + $", Steps: {Steps}";
}
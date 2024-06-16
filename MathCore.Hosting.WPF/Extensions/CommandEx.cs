using MathCore.WPF.Commands;
using Microsoft.Extensions.Logging;

namespace MathCore.Hosting.WPF.Extensions;

public static class CommandEx
{
    public static TCommand WithLogging<TCommand>(this TCommand command, ILogger logger) where TCommand : Command
    {
        command.BeforeExecuted += BeforeCommandExecuting;
        command.Executed += OnCommandExecuted;
        command.Error += OnCommandExecutingError;

        return command;

        void BeforeCommandExecuting(object? Sender, EventArgs<object?> E)
        {
            logger.LogInformation("Command {0} start executing with parameter {1}", command, E.Argument);
        }

        void OnCommandExecuted(object? Sender, EventArgs<object?> E)
        {
            logger.LogInformation("Command {0} executed successful with parameter {1}", command, E.Argument);
        }

        void OnCommandExecutingError(object Sender, ExceptionEventHandlerArgs<Exception> Args)
        {
            logger.LogError("Command {0} thrown error {1}", command, Args.Argument);
        }
    }
}

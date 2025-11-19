#nullable enable
using MathCore.WPF.Commands;
using Microsoft.Extensions.Logging;

namespace MathCore.Hosting.WPF.Extensions;

/// <summary>Методы расширения для конфигурации команд</summary>
public static class CommandEx
{
    /// <summary>Добавляет логирование событий выполнения команды</summary>
    /// <param name="command">Команда к которой добавляется логирование</param>
    /// <param name="logger">Логгер используемый для записи событий команды</param>
    /// <returns>Исходная команда с подключёнными обработчиками логирования</returns>
    public static TCommand WithLogging<TCommand>(this TCommand command, ILogger logger) where TCommand : Command
    {
        command.BeforeExecuted += BeforeCommandExecuting;
        command.Executed += OnCommandExecuted;
        command.Error += OnCommandExecutingError;

        return command;

        void BeforeCommandExecuting(object? Sender, EventArgs<object?> E) => logger.LogInformation("Command {command} start executing with parameter {parameter}", command, E.Argument); // логируем старт выполнения

        void OnCommandExecuted(object? Sender, EventArgs<object?> E) => logger.LogInformation("Command {command} executed successful with parameter {parameter}", command, E.Argument); // логируем успешное выполнение

        void OnCommandExecutingError(object Sender, ExceptionEventHandlerArgs<Exception> Args) => logger.LogError("Command {command} thrown error {exception}", command, Args.Argument); // логируем ошибку выполнения
    }
}

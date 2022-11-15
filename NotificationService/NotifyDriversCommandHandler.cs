using RideSharingMessages;
using Spectre.Console;

namespace NotificationService;

public static class NotifyDriversCommandHandler
{
    public static void Handle(NotifyDriversCommand command)
    {
        AnsiConsole.MarkupLine($"[blue]I got a message to notify some drivers about a new ride...[/]");
    }
}
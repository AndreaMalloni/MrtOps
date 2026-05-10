using System;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace MrtOps.WPF.Logging;

public class UiConsoleSink : ILogEventSink
{
    public static event Action<string>? OnLogReceived;

    private readonly ITextFormatter _formatter = new MessageTemplateTextFormatter(
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

    public void Emit(LogEvent logEvent)
    {
        if (OnLogReceived == null) return;

        using var renderSpace = new StringWriter();
        _formatter.Format(logEvent, renderSpace);

        OnLogReceived.Invoke(renderSpace.ToString());
    }
}
using Microsoft.Extensions.DependencyInjection;
using MrtOps.Core;
using MrtOps.WPF;
using MrtOps.WPF.Logging;
using MrtOps.WPF.ViewModels;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using System.Windows;

namespace MrtOps;

public partial class App : Application
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        string logTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{SourceContext}] [ThreadId {ThreadId}] {Message:lj}{NewLine}{Exception}";

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithMachineName()
            .Enrich.WithExceptionDetails() 

            .WriteTo.Logger(l => l
                .Filter.ByIncludingOnly(evt => evt.Level <= LogEventLevel.Information)
                .WriteTo.File("logs/mrtops-info-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: logTemplate,
                    retainedFileCountLimit: 7)) 

            .WriteTo.Logger(l => l
                .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Warning)
                .WriteTo.File("logs/mrtops-warnings-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: logTemplate))

            .WriteTo.Logger(l => l
                .Filter.ByIncludingOnly(evt => evt.Level >= LogEventLevel.Error)
                .WriteTo.File("logs/mrtops-errors-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: logTemplate))

            .WriteTo.Logger(l => l
                .Filter.ByIncludingOnly(evt => evt.Level >= LogEventLevel.Information)
                .WriteTo.Sink(new UiConsoleSink()))

            .CreateLogger();

        SetupGlobalExceptionHandling();

        Log.Information("=== MrtOps Application Starting ===");

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddSerilog(dispose: true));

        services.AddSingleton<BatchProcessingService>();

        services.AddTransient<MainViewModel>();

        ServiceProvider = services.BuildServiceProvider();

        var mainWindow = new MainWindow(
            ServiceProvider.GetRequiredService<MainViewModel>()
        );

        mainWindow.Show();
    }

    private void SetupGlobalExceptionHandling()
    {
        DispatcherUnhandledException += (s, e) =>
        {
            Log.Fatal(e.Exception, "CRASH: Rilevata eccezione non gestita nel thread UI Dispatcher.");
            e.Handled = true; 
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            Log.Fatal(e.Exception, "CRASH: Rilevata eccezione non gestita in un Task in background.");
            e.SetObserved();
        };

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                Log.Fatal(ex, "FATAL: Eccezione di dominio non gestita. L'applicazione sta per chiudersi.");
            }
            Log.CloseAndFlush();
        };
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("=== MrtOps Application Shutting Down ===");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
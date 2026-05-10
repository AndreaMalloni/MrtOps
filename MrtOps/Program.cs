using Microsoft.Extensions.DependencyInjection;
using MrtOps.CLI;
using MrtOps.CLI.Commands;
using MrtOps.Core;
using MrtOps.Core.Interfaces;
using MrtOps.Core.Storage;
using MrtOps.WPF;
using MrtOps.WPF.Logging;
using MrtOps.WPF.ViewModels;
using Serilog;
using Serilog.Events;
using Spectre.Console.Cli;
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace MrtOps;

public class Program
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(int processId);
    private const int AttachParentProcess = -1;

    [STAThread]
    public static int Main(string[] args)
    {
        bool isCliMode = args.Length > 0;

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level <= LogEventLevel.Information)
                                  .WriteTo.File("logs/mrtops-info-.txt", rollingInterval: RollingInterval.Day))
            .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                                  .WriteTo.File("logs/mrtops-warnings-.txt", rollingInterval: RollingInterval.Day))
            .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
                                  .WriteTo.File("logs/mrtops-errors-.txt", rollingInterval: RollingInterval.Day));

        if (isCliMode)
            loggerConfig.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
        else
            loggerConfig.WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Information)
                                              .WriteTo.Sink(new UiConsoleSink()));

        Log.Logger = loggerConfig.CreateLogger();

        try
        {
            var services = new ServiceCollection();

            services.AddLogging(builder => builder.AddSerilog(dispose: true));
            services.AddSingleton<ILocalizationService, LocalizationService>();
            services.AddSingleton<OperationHistoryManager>();
            services.AddSingleton<ITemplateRepository, FileTemplateRepository>();
            services.AddSingleton<IReportEngine, StimulsoftReportEngine>();
            services.AddSingleton<BatchProcessingService>();

            if (isCliMode)
            {
                AttachConsole(AttachParentProcess);

                var registrar = new TypeRegistrar(services);
                var app = new CommandApp(registrar);

                app.Configure(config =>
                {
                    config.AddCommand<GenerateCommand>("gen");
                    config.AddCommand<BatchCommand>("batch");
                    config.AddCommand<DbScanCommand>("db-scan");
                    config.AddCommand<SyncStyleCommand>("sync-style");
                    config.AddCommand<SyncStringsCommand>("sync-strings");
                    config.AddCommand<UndoCommand>("undo");
                });

                return app.Run(args);
            }
            else
            {
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainWindow>();

                var serviceProvider = services.BuildServiceProvider();

                var wpfApp = new System.Windows.Application();
                var mainWindow = serviceProvider.GetRequiredService<MainWindow>();

                wpfApp.Run(mainWindow);
                return 0;
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Errore fatale imprevisto che ha causato il crash dell'applicazione.");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
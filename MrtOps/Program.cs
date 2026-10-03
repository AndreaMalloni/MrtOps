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

/// <summary>
/// The main entry point for the application.
/// </summary>
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
            services.AddSingleton<IHistoryStorage, JsonHistoryStorage>();
            services.AddSingleton<OperationHistoryManager>();
            services.AddSingleton<ITemplateRepository, FileTemplateRepository>();
            services.AddSingleton<IReportEngine, StimulsoftReportEngine>();
            services.AddSingleton<BatchProcessingService>();
            services.AddSingleton<IServerConfigurationService, ServerConfigurationService>();
            services.AddSingleton<IDatabaseService, MultiDatabaseService>();

            if (isCliMode)
            {
                AttachConsole(AttachParentProcess);

                // Fix for WinExe: AttachConsole does not automatically update Console.In
                // We must manually reinitialize the standard input stream so Spectre.Console can read interactively
                var stdIn = new System.IO.StreamReader(Console.OpenStandardInput(), Console.InputEncoding);
                Console.SetIn(stdIn);

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

                    config.AddBranch("server", server =>
                    {
                        server.AddCommand<ServerAddCommand>("add");
                        server.AddCommand<ServerUpdateCommand>("edit");
                        server.AddCommand<ServerListCommand>("list");
                        server.AddCommand<ServerRemoveCommand>("remove");
                        server.AddCommand<ServerExportCommand>("export");
                        server.AddCommand<ServerImportCommand>("import");
                    });
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
            Log.Fatal(ex, "An unexpected fatal error occurred, causing the application to crash.");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
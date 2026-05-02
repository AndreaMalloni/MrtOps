using Microsoft.Extensions.DependencyInjection;
using MrtOps.Application.Services;
using MrtOps.Core.History;
using MrtOps.Core.Interfaces;
using MrtOps.Infrastructure.Localization;
using MrtOps.Infrastructure.Stimulsoft;
using MrtOps.Infrastructure.Storage;
using MrtOps.Presentation.CLI.Commands;
using MrtOps.Presentation.CLI.Infrastructure;
using MrtOps.Presentation.WPF;
using MrtOps.Presentation.WPF.ViewModels;
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
        var services = new ServiceCollection();

        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<OperationHistoryManager>();
        services.AddSingleton<ITemplateRepository, JsonTemplateRepository>();
        services.AddSingleton<IReportEngine, StimulsoftReportEngine>();
        services.AddSingleton<BatchProcessingService>();

        if (args.Length > 0)
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
}
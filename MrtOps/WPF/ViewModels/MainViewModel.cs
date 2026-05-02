using System.Windows;
using MrtOps.Core.Interfaces;

namespace MrtOps.Presentation.WPF.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ILocalizationService _loc;
    private bool _isPreviewEnabled = true;
    private bool _isDarkMode = false;
    private string _currentSection = "Batch";
    private ViewModelBase _currentViewModel;
    private string _consoleOutput = "MrtOps initialized...\n";

    public bool IsPreviewEnabled
    {
        get => _isPreviewEnabled;
        set => SetProperty(ref _isPreviewEnabled, value);
    }

    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (SetProperty(ref _isDarkMode, value)) ApplyTheme();
        }
    }

    public string CurrentSection
    {
        get => _currentSection;
        set => SetProperty(ref _currentSection, value);
    }

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public string ConsoleOutput
    {
        get => _consoleOutput;
        set => SetProperty(ref _consoleOutput, value);
    }

    public RelayCommand ToggleThemeCommand { get; }
    public RelayCommand NavigateCommand { get; }

    public MainViewModel(ILocalizationService loc)
    {
        _loc = loc;
        _currentViewModel = new BatchProcessingViewModel();

        ToggleThemeCommand = new RelayCommand(_ => IsDarkMode = !IsDarkMode);
        NavigateCommand = new RelayCommand(ExecuteNavigate);

        ApplyTheme();
    }

    private void ExecuteNavigate(object? parameter)
    {
        if (parameter is not string section) return;

        CurrentSection = section;
        CurrentViewModel = section switch
        {
            "Dashboard" => new DashboardViewModel(),
            "Batch" => new BatchProcessingViewModel(),
            _ => new DashboardViewModel()
        };
    }

    private void ApplyTheme()
    {
        var app = System.Windows.Application.Current;
        if (app == null) return;

        app.Resources.MergedDictionaries.Clear();

        app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new System.Uri($"/Presentation/WPF/Themes/{(IsDarkMode ? "DarkTheme" : "LightTheme")}.xaml", System.UriKind.Relative) });
        app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new System.Uri("/Presentation/WPF/Styles/Controls.xaml", System.UriKind.Relative) });
    }
}
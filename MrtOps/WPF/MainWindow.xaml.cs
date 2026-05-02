using System.Windows;
using MrtOps.Presentation.WPF.ViewModels;

namespace MrtOps.Presentation.WPF;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
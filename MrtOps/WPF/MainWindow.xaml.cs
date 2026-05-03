using System.Windows;
using MrtOps.WPF.ViewModels;

namespace MrtOps.WPF;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
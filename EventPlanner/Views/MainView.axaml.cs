using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EventPlanner.ViewModels;

namespace EventPlanner;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
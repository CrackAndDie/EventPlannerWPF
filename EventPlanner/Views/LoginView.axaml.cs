using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EventPlanner.ViewModels;

namespace EventPlanner;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
        DataContext = new LoginViewModel();
    }
}
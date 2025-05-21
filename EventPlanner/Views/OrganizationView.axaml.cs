using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EventPlanner.ViewModels;

namespace EventPlanner;

public partial class OrganizationView : UserControl
{
    public OrganizationView()
    {
        InitializeComponent();
        DataContext = new OrganizationViewModel();
    }
}
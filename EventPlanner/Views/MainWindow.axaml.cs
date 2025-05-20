using Avalonia.Controls;
using EventPlanner.ViewModels;

namespace EventPlanner.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            App.CurrentWindowViewModel = new MainWindowViewModel();
            DataContext = App.CurrentWindowViewModel;
        }
    }
}
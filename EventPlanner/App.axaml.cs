using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EventPlanner.Entities;
using EventPlanner.Managers;
using EventPlanner.ViewModels;
using EventPlanner.Views;

namespace EventPlanner
{
    public partial class App : Application
    {
        // cringe :)
        public static DbContextManager DbContext { get; private set; } = new DbContextManager();
        public static MainWindowViewModel CurrentWindowViewModel { get; set; }
        public static User CurrentUser { get; set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
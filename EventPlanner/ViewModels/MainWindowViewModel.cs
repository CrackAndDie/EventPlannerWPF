using Avalonia.Controls;
using ReactiveUI.Fody.Helpers;

namespace EventPlanner.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            CurrentView = new LoginView();
        }

        public void ChangeView(UserControl view)
        {
            CurrentView = view;
        }

        [Reactive]
        public UserControl CurrentView { get; set; }
    }
}

using System.Windows.Input;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace EventPlanner.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel() 
        {
            AddEventCommand = ReactiveCommand.Create(OnAddEventCommand);
            SelectEventCommand = ReactiveCommand.Create<EventDTO>(OnSelectEventCommand);

            var theUser = App.CurrentUser;
            UserName = $"ФИО: {theUser.FullName}";
            UserRole = $"Роль: {theUser.GetRole(App.DbContext).Name}";
            UserOrg  = $"Организация: {theUser.GetOrg(App.DbContext).Name}";
        }

        private void OnSelectEventCommand(EventDTO eventData)
        {

        }

        private void OnAddEventCommand()
        {
            App.CurrentWindowViewModel.ChangeView(new EventView() { DataContext = new EventViewModel(null) });
        }

        [Reactive]
        public string UserName { get; set; }
        [Reactive]
        public string UserRole { get; set; }
        [Reactive]
        public string UserOrg { get; set; }

        [Reactive]
        public ICommand AddEventCommand { get; set; }
        [Reactive]
        public ICommand SelectEventCommand { get; set; }
    }

    public class EventDTO
    {
        public string Name { get; set; }
        public Bitmap Image { get; set; }
    }
}

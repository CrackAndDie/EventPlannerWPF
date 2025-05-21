using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using DynamicData;
using EventPlanner.Entities;
using EventPlanner.Helpers;
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

            // set up events
            AllEvents.AddRange(App.DbContext.Events.Select(x => new EventDTO() 
            { 
                Name = x.Name, 
                Image = x.Photo == null ? null : ImageConverter.ByteArrayToImage(x.Photo), 
                OriginalEvent = x,
            }));
        }

        private void OnSelectEventCommand(EventDTO eventData)
        {
            App.CurrentWindowViewModel.ChangeView(new EventView() { DataContext = new EventViewModel(eventData.OriginalEvent) });
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

        public ObservableCollection<EventDTO> AllEvents { get; set; } = new ObservableCollection<EventDTO>();

        [Reactive]
        public ICommand AddEventCommand { get; set; }
        [Reactive]
        public ICommand SelectEventCommand { get; set; }
    }

    public class EventDTO
    {
        public string Name { get; set; }
        public Bitmap Image { get; set; }
        public Event OriginalEvent { get; set; }
    }
}

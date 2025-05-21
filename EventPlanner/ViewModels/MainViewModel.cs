using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using DynamicData;
using EventPlanner.Entities;
using EventPlanner.Helpers;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

            // filtering
            IQueryable<Event> allowedEvents = App.DbContext.Events;
            allowedEvents = allowedEvents.Where(x => x.OrganizationId == App.CurrentUser.OrganizationId);

            // set up events
            AllEvents.AddRange(allowedEvents.Select(x => new EventDTO() 
            { 
                Name = x.Name, 
                Image = x.Photo == null ? null : ImageConverter.ByteArrayToImage(x.Photo), 
                OriginalEvent = x,
            }));
        }

        private void OnSelectEventCommand(EventDTO eventData)
        {
            var theView = new EventView();
            theView.DataContext = new EventViewModel(eventData.OriginalEvent, theView);
            App.CurrentWindowViewModel.ChangeView(theView);
        }

        private void OnAddEventCommand()
        {
            var theView = new EventView();
            theView.DataContext = new EventViewModel(null, theView);
            App.CurrentWindowViewModel.ChangeView(theView);
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

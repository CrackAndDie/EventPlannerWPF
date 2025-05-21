using System;
using System.IO;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using EventPlanner.Entities;
using EventPlanner.Helpers;
using EventPlanner.Views;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using DynamicData;
using System.Linq;

namespace EventPlanner.ViewModels
{
    public class EventViewModel : ViewModelBase
    {
        public EventViewModel(Event theEvent, EventView theView) 
        {
            SelectImageCommand = ReactiveCommand.Create(OnSelectImageCommand);
            SaveCommand = ReactiveCommand.Create(OnSaveCommand);
            CancelCommand = ReactiveCommand.Create(OnCancelCommand);
            SelectEventTaskCommand = ReactiveCommand.Create<EventTaskDTO>(OnSelectEventTaskCommand);

            _currentEvent = theEvent;
            _currentView = theView;
            if (_currentEvent != null)
            {
                EventName = _currentEvent.Name;
                EventDesc = _currentEvent.Description;
                DateStart = _currentEvent.StartDate;
                TimeStart = _currentEvent.StartDate.TimeOfDay;
                DateEnd = _currentEvent.EndDate;
                TimeEnd = _currentEvent.EndDate.TimeOfDay;
                EventImage = _currentEvent.Photo == null ? null : ImageConverter.ByteArrayToImage(_currentEvent.Photo);

                UpdateTasks();
            }
        }

        async private void OnSelectImageCommand()
        {
            var window = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow;
            // Start async operation to open the dialog.
            var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Image File",
                AllowMultiple = false
            });

            if (files.Count == 1)
            {
                // Open reading stream from the first file.
                await using var stream = await files[0].OpenReadAsync();
                var data = StreamHelper.ReadFully(stream);

                try
                {
                    EventImage = ImageConverter.ByteArrayToImage(data);
                }
                catch
                {
                    var box = MessageBoxManager
                       .GetMessageBoxStandard("Ошибка", "Ошибка при открытии изображения!",
                       ButtonEnum.Ok);

                    await box.ShowAsync();
                }
            }
        }

        async private void OnSaveCommand()
        {
            if (!Checks())
            {
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Ошибка", "Имя, дата/время начала, дата/время конца - обязательные поля для заполнения!",
                    ButtonEnum.Ok);

                await box.ShowAsync();
                return;
            }

            bool isAdd = false;
            if (_currentEvent == null)
            {
                _currentEvent = new Event();
                isAdd = true;
            }

            _currentEvent.Name = EventName;
            _currentEvent.Description = EventDesc;
            var dateStart = new DateTime(DateStart.Value.Year, DateStart.Value.Month, DateStart.Value.Day, TimeStart.Value.Hours, TimeStart.Value.Minutes, TimeStart.Value.Seconds);
            _currentEvent.StartDate = dateStart;
            var dateEnd = new DateTime(DateEnd.Value.Year, DateEnd.Value.Month, DateEnd.Value.Day, TimeEnd.Value.Hours, TimeEnd.Value.Minutes, TimeEnd.Value.Seconds);
            _currentEvent.EndDate = dateEnd;
            _currentEvent.Photo = EventImage == null ? null : ImageConverter.ImageToByteArray(EventImage);

            if (isAdd)
            {
                _currentEvent.OrganizationId = App.CurrentUser.OrganizationId;
                App.DbContext.Events.Add(_currentEvent);
            }
            else
            {
                App.DbContext.Entry(_currentEvent).State = EntityState.Modified;
            }
            await App.DbContext.SaveChangesAsync();

            var box2 = MessageBoxManager
                    .GetMessageBoxStandard("Успешно", "Мероприятие сохранено!",
                    ButtonEnum.Ok);
            await box2.ShowAsync();

            // and go back
            OnCancelCommand();
        }

        private void OnCancelCommand()
        {
            // just go back
            App.CurrentWindowViewModel.ChangeView(new MainView());
        }

        private void OnAddEventTaskCommand()
        {
            var theView = new EventTaskView();
            theView.DataContext = new EventTaskViewModel(null, theView, _currentView, _currentEvent);
            App.CurrentWindowViewModel.ChangeView(theView);
        }

        private void OnSelectEventTaskCommand(EventTaskDTO eventTask)
        {
            var theView = new EventTaskView();
            theView.DataContext = new EventTaskViewModel(eventTask.OriginalEventTask, theView, _currentView, _currentEvent);
            App.CurrentWindowViewModel.ChangeView(theView);
        }

        public void UpdateTasks()
        {
            AllEventTasks.Clear();

            IQueryable<EventTask> eventTasks = App.DbContext.EventTasks;
            eventTasks.Where(x => x.EventId == _currentEvent.Id);

            AllEventTasks.AddRange(eventTasks.Select(x => new EventTaskDTO()
            {
                Name = x.Name,
                State = x.GetState(App.DbContext).Name,
                OriginalEventTask = x,
            }));
        }

        private bool Checks()
        {
            if (string.IsNullOrWhiteSpace(EventName))
                return false;
            if (!DateStart.HasValue || !DateEnd.HasValue || !TimeStart.HasValue || !TimeEnd.HasValue)
                return false;
            return true;
        }

        private Event _currentEvent;
        private EventView _currentView;

        [Reactive]
        public string EventName { get; set; }
        [Reactive]
        public string EventDesc { get; set; }
        [Reactive]
        public DateTime? DateStart { get; set; }
        [Reactive]
        public TimeSpan? TimeStart { get; set; }
        [Reactive]
        public DateTime? DateEnd { get; set; }
        [Reactive]
        public TimeSpan? TimeEnd { get; set; }
        [Reactive]
        public Bitmap EventImage { get; set; }

        public ObservableCollection<EventTaskDTO> AllEventTasks { get; set; } = new ObservableCollection<EventTaskDTO>();

        [Reactive]
        public ICommand SelectImageCommand { get; set; }
        [Reactive]
        public ICommand SaveCommand { get; set; }
        [Reactive]
        public ICommand CancelCommand { get; set; }
        [Reactive]
        public ICommand SelectEventTaskCommand { get; set; }
    }

    public class EventTaskDTO
    {
        public string Name { get; set; }
        public string State { get; set; }
        public EventTask OriginalEventTask { get; set; }
    }
}

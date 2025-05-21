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

namespace EventPlanner.ViewModels
{
    public class EventViewModel : ViewModelBase
    {
        public EventViewModel(Event theEvent) 
        {
            SelectImageCommand = ReactiveCommand.Create(OnSelectImageCommand);
            SaveCommand = ReactiveCommand.Create(OnSaveCommand);
            CancelCommand = ReactiveCommand.Create(OnCancelCommand);

            _currentEvent = theEvent;
            if (_currentEvent != null)
            {
                EventName = _currentEvent.Name;
                EventDesc = _currentEvent.Description;
                DateStart = _currentEvent.StartDate;
                TimeStart = _currentEvent.StartDate.TimeOfDay;
                DateEnd = _currentEvent.EndDate;
                TimeEnd = _currentEvent.EndDate.TimeOfDay;
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
                EventImage = ImageConverter.ByteArrayToImage(data);
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
            var dateEnd = new DateTime(DateStart.Value.Year, DateStart.Value.Month, DateStart.Value.Day, TimeStart.Value.Hours, TimeStart.Value.Minutes, TimeStart.Value.Seconds);
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

        private bool Checks()
        {
            if (string.IsNullOrWhiteSpace(EventName))
                return false;
            if (!DateStart.HasValue || !DateEnd.HasValue || !TimeStart.HasValue || !TimeEnd.HasValue)
                return false;
            return true;
        }

        private Event _currentEvent;

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

        [Reactive]
        public ICommand SelectImageCommand { get; set; }
        [Reactive]
        public ICommand SaveCommand { get; set; }
        [Reactive]
        public ICommand CancelCommand { get; set; }
    }
}

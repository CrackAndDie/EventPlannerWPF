using DynamicData;
using EventPlanner.Entities;
using EventPlanner.Helpers;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace EventPlanner.ViewModels
{
    public class EventTaskViewModel : ViewModelBase
    {
        public EventTaskViewModel(EventTask theEventTask, EventTaskView eventTaskView, EventView eventView, Event theEvent)
        {
            RemoveUserCommand = ReactiveCommand.Create<UserDTO>(OnRemoveUserCommand);
            SaveCommand = ReactiveCommand.Create(OnSaveCommand);
            CancelCommand = ReactiveCommand.Create(OnCancelCommand);

            _currentEventTask = theEventTask;
            _currentEventView = eventView;
            _currentEventTaskView = eventTaskView;
            _currentEvent = theEvent;
            if (_currentEventTask != null)
            {
                EventTaskName = _currentEventTask.Name;
                EventTaskDesc = _currentEventTask.Description;
                DateStart = _currentEventTask.StartDate;
                TimeStart = _currentEventTask.StartDate.TimeOfDay;
                PlanDateEnd = _currentEventTask.PlanEndDate;
                PlanTimeEnd = _currentEventTask.PlanEndDate.TimeOfDay;

                ActualDateEnd = _currentEventTask.ActualEndDate;
                ActualTimeEnd = _currentEventTask.ActualEndDate.HasValue ? _currentEventTask.ActualEndDate.Value.TimeOfDay : null;

                UpdateUsers();
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
            if (_currentEventTask == null)
            {
                _currentEventTask = new EventTask();
                isAdd = true;
            }

            _currentEventTask.Name = EventTaskName;
            _currentEventTask.Description = EventTaskDesc;
            var dateStart = new DateTime(DateStart.Value.Year, DateStart.Value.Month, DateStart.Value.Day, TimeStart.Value.Hours, TimeStart.Value.Minutes, TimeStart.Value.Seconds);
            _currentEventTask.StartDate = dateStart;
            var dateEnd = new DateTime(PlanDateEnd.Value.Year, PlanDateEnd.Value.Month, PlanDateEnd.Value.Day, PlanTimeEnd.Value.Hours, PlanTimeEnd.Value.Minutes, PlanTimeEnd.Value.Seconds);
            _currentEventTask.PlanEndDate = dateEnd;
            if (ActualDateEnd.HasValue && ActualTimeEnd.HasValue)
            {
                var dateEndActual = new DateTime(ActualDateEnd.Value.Year, ActualDateEnd.Value.Month, ActualDateEnd.Value.Day, ActualTimeEnd.Value.Hours, ActualTimeEnd.Value.Minutes, ActualTimeEnd.Value.Seconds);
                _currentEventTask.ActualEndDate = dateEndActual;
            }

            if (isAdd)
            {
                _currentEventTask.EventId = _currentEvent.Id;
                App.DbContext.EventTasks.Add(_currentEventTask);
            }
            else
            {
                App.DbContext.Entry(_currentEventTask).State = EntityState.Modified;
            }
            await App.DbContext.SaveChangesAsync();

            var box2 = MessageBoxManager
                    .GetMessageBoxStandard("Успешно", "Задание сохранено!",
                    ButtonEnum.Ok);
            await box2.ShowAsync();

            // and go back
            OnCancelCommand();
        }

        private void OnCancelCommand()
        {
            // just go back
            App.CurrentWindowViewModel.ChangeView(_currentEventView);
        }

        private void OnRemoveUserCommand(UserDTO user)
        {
            
        }

        public void OnAddUserCommand()
        {

        }

        public void UpdateUsers()
        {
            IQueryable<User> eventTaskUsers = App.DbContext.Users;
            IQueryable<UserEventTask> userTasks = App.DbContext.UserEventTasks;
            var userIds = userTasks.Where(x => x.EventTaskId == _currentEventTask.Id).Select(x => x.UserId);
            eventTaskUsers = eventTaskUsers.Where(x => userIds.Contains(x.Id));

            AllUsers.AddRange(eventTaskUsers.Select(x => new UserDTO()
            {
                FullName = x.FullName,
                Role = x.GetRole(App.DbContext).Name,
                OriginalUser = x,
            }));
        }

        private bool Checks()
        {
            if (string.IsNullOrWhiteSpace(EventTaskName))
                return false;
            if (!DateStart.HasValue || !PlanDateEnd.HasValue || !TimeStart.HasValue || !PlanTimeEnd.HasValue)
                return false;
            return true;
        }

        private Event _currentEvent;
        private EventTask _currentEventTask;
        private EventView _currentEventView;
        private EventTaskView _currentEventTaskView;

        [Reactive]
        public string EventTaskName { get; set; }
        [Reactive]
        public string EventTaskDesc { get; set; }
        [Reactive]
        public DateTime? DateStart { get; set; }
        [Reactive]
        public TimeSpan? TimeStart { get; set; }
        [Reactive]
        public DateTime? PlanDateEnd { get; set; }
        [Reactive]
        public TimeSpan? PlanTimeEnd { get; set; }
        [Reactive]
        public DateTime? ActualDateEnd { get; set; }
        [Reactive]
        public TimeSpan? ActualTimeEnd { get; set; }

        public ObservableCollection<UserDTO> AllUsers { get; set; } = new ObservableCollection<UserDTO>();

        [Reactive]
        public ICommand SaveCommand { get; set; }
        [Reactive]
        public ICommand CancelCommand { get; set; }
        [Reactive]
        public ICommand RemoveUserCommand { get; set; }
    }

    public class UserDTO
    {
        public string FullName { get; set; }
        public string Role { get; set; }
        public User OriginalUser { get; set; }
    }
}

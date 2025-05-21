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
            AddUserCommand = ReactiveCommand.Create(OnAddUserCommand);
            RemoveUserCommand = ReactiveCommand.Create<UserDTO>(OnRemoveUserCommand);
            SaveCommand = ReactiveCommand.Create(OnSaveCommand);
            CancelCommand = ReactiveCommand.Create(OnCancelCommand);

            IsTaskEditAllowed = (App.CurrentUser.GetRoleEnum(App.DbContext) == RoleEnum.Admin) ||
                (App.CurrentUser.GetRoleEnum(App.DbContext) == RoleEnum.Director) ||
                (App.CurrentUser.GetRoleEnum(App.DbContext) == RoleEnum.TaskManager);
            IsUsersEditAllowed = (theEventTask != null) && // allow add only for existing
                ((App.CurrentUser.GetRoleEnum(App.DbContext) == RoleEnum.Admin) ||
                (App.CurrentUser.GetRoleEnum(App.DbContext) == RoleEnum.Director) ||
                (App.CurrentUser.GetRoleEnum(App.DbContext) == RoleEnum.StaffManager));

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

                SelectedState = _currentEventTask.GetState(App.DbContext);

                ActualDateEnd = _currentEventTask.ActualEndDate;
                ActualTimeEnd = _currentEventTask.ActualEndDate.HasValue ? _currentEventTask.ActualEndDate.Value.TimeOfDay : null;

                UpdateUsers();
            }

            AllStatuses.AddRange(App.DbContext.TaskStates.ToList());
        }

        async private void OnSaveCommand()
        {
            if (!Checks())
            {
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Ошибка", "Имя, дата/время начала, дата/время конца, статус - обязательные поля для заполнения!",
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
            _currentEventTask.StateId = SelectedState.Id;

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

            (_currentEventView.DataContext as EventViewModel).UpdateTasks();
            // and go back
            OnCancelCommand();
        }

        private void OnCancelCommand()
        {
            // just go back
            App.CurrentWindowViewModel.ChangeView(_currentEventView);
        }

        async private void OnRemoveUserCommand(UserDTO user)
        {
            var entry = App.DbContext.UserEventTasks.FirstOrDefault(x => x.UserId == user.OriginalUser.Id && x.EventTaskId == _currentEventTask.Id);
            if (entry != null)
            {
                var box2 = MessageBoxManager
                    .GetMessageBoxStandard("Удаление", "Удалить?",
                    ButtonEnum.YesNo);
                var result = await box2.ShowAsync();

                if (result == ButtonResult.Yes)
                {
                    App.DbContext.UserEventTasks.Remove(entry);
                    App.DbContext.SaveChanges();
                    UpdateUsers();
                }
            }
        }

        public void OnAddUserCommand()
        {
            var eventTaskUsers = App.DbContext.Users.ToList();
            var userTasks = App.DbContext.UserEventTasks.ToList();
            var userIds = userTasks.Where(x => x.EventTaskId == _currentEventTask.Id).Select(x => x.UserId).ToList();
            eventTaskUsers = eventTaskUsers.Where(x => !userIds.Contains(x.Id) && x.OrganizationId == App.CurrentUser.OrganizationId).ToList();

            var usersDto = eventTaskUsers.Select(x => new UserDTO()
            {
                FullName = x.FullName,
                Role = x.GetRole(App.DbContext).Name,
                OriginalUser = x,
            }).ToList();

            var theView = new AllUsersView();
            theView.DataContext = new AllUsersViewModel(theView, _currentEventTaskView, usersDto);
            App.CurrentWindowViewModel.ChangeView(theView);
        }

        public void AddUserCallback(UserDTO user)
        {
            var userEventTask = new UserEventTask() { UserId = user.OriginalUser.Id, EventTaskId = _currentEventTask.Id };
            App.DbContext.UserEventTasks.Add(userEventTask);
            App.DbContext.SaveChanges();
        }

        public void UpdateUsers()
        {
            AllUsers.Clear();

            var eventTaskUsers = App.DbContext.Users.ToList();
            var userTasks = App.DbContext.UserEventTasks.ToList();
            var userIds = userTasks.Where(x => x.EventTaskId == _currentEventTask.Id).Select(x => x.UserId).ToList();
            eventTaskUsers = eventTaskUsers.Where(x => userIds.Contains(x.Id)).ToList();

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
            if (SelectedState == null)
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

        [Reactive]
        public bool IsTaskEditAllowed { get; set; }
        [Reactive]
        public bool IsUsersEditAllowed { get; set; }

        [Reactive]
        public TaskState SelectedState { get; set; }

        public ObservableCollection<UserDTO> AllUsers { get; set; } = new ObservableCollection<UserDTO>();
        public ObservableCollection<TaskState> AllStatuses { get; set; } = new ObservableCollection<TaskState>();

        [Reactive]
        public ICommand SaveCommand { get; set; }
        [Reactive]
        public ICommand CancelCommand { get; set; }
        [Reactive]
        public ICommand AddUserCommand { get; set; }
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

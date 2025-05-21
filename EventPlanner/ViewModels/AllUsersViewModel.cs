using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DynamicData;
using EventPlanner.Entities;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace EventPlanner.ViewModels
{
    public class AllUsersViewModel : ViewModelBase
    {
        public AllUsersViewModel(AllUsersView theView, EventTaskView eventTaskView, List<UserDTO> usersToShow)
        {
            AddUserCommand = ReactiveCommand.Create(OnAddUserCommand);
            CancelCommand = ReactiveCommand.Create(OnCancelCommand);
            RemoveUserCommand = ReactiveCommand.Create<UserDTO>(OnRemoveUserCommand);
            SelectUserCommand = ReactiveCommand.Create<UserDTO>(OnSelectUserCommand);

            _currentView = theView;
            _currentEventTaskView = eventTaskView;
            if (usersToShow != null)
            {
                AllUsers.AddRange(usersToShow);
                IsRemoveEnabled = false;
            }
            else
            {
                UpdateUsers();
            }
        }

        private void OnAddUserCommand()
        {
            var theView = new UserView();
            theView.DataContext = new UserViewModel(_currentView, null);
            App.CurrentWindowViewModel.ChangeView(theView);
        }

        private void OnRemoveUserCommand(UserDTO user)
        {

        }

        private void OnSelectUserCommand(UserDTO user)
        {
            if (_currentEventTaskView != null)
            {
                (_currentEventTaskView.DataContext as EventTaskViewModel).AddUserCallback(user);
                (_currentEventTaskView.DataContext as EventTaskViewModel).UpdateUsers();
                App.CurrentWindowViewModel.ChangeView(_currentEventTaskView);
            }
            else
            {
                var theView = new UserView();
                theView.DataContext = new UserViewModel(_currentView, user.OriginalUser);
                App.CurrentWindowViewModel.ChangeView(theView);
            }
        }

        private void OnCancelCommand()
        {
            if (_currentEventTaskView != null)
            {
                App.CurrentWindowViewModel.ChangeView(_currentEventTaskView);
            }
            else
            {
                App.CurrentWindowViewModel.ChangeView(new MainView());
            }
        }

        public void UpdateUsers()
        {
            AllUsers.Clear();

            var users = App.DbContext.Users.ToList();
            var usersDto = users.Select(x => new UserDTO()
            {
                FullName = x.FullName,
                Role = x.GetRole(App.DbContext).Name,
                OriginalUser = x,
            });
            if (App.CurrentUser.GetRoleEnum(App.DbContext) == RoleEnum.Admin)
            {
                AllUsers.AddRange(usersDto);
            }
            else
            {
                usersDto = usersDto.Where(x => x.OriginalUser.OrganizationId == App.CurrentUser.OrganizationId);
                AllUsers.AddRange(usersDto);
            }
        }

        private EventTaskView _currentEventTaskView;
        private AllUsersView _currentView;

        [Reactive]
        public bool IsRemoveEnabled { get; set; } = true;

        public ObservableCollection<UserDTO> AllUsers { get; set; } = new ObservableCollection<UserDTO>();

        [Reactive]
        public ICommand AddUserCommand { get; set; }
        [Reactive]
        public ICommand RemoveUserCommand { get; set; }
        [Reactive]
        public ICommand SelectUserCommand { get; set; }
        [Reactive]
        public ICommand CancelCommand { get; set; }
    }
}

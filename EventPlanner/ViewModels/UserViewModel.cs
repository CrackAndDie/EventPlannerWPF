using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DynamicData;
using EventPlanner.Entities;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Xml.Linq;

namespace EventPlanner.ViewModels
{
    public class UserViewModel : ViewModelBase
    {
        public UserViewModel(AllUsersView usersView, User user) 
        {
            SaveCommand = ReactiveCommand.Create(OnSaveCommand);
            CancelCommand = ReactiveCommand.Create(OnCancelCommand);

            AllRoles.AddRange(App.DbContext.Roles.ToList());
            AllOrgs.AddRange(App.DbContext.Organizations.ToList());

            _usersView = usersView;
            _currentUser = user;

            if (_currentUser != null)
            {
                FullName = _currentUser.FullName;
                Login = _currentUser.Login;
                PhoneNumber = _currentUser.PhoneNumber;
                SelectedOrg = _currentUser.GetOrg(App.DbContext);
                SelectedRole = _currentUser.GetRole(App.DbContext);
            }

            if (App.CurrentUser.GetRoleEnum(App.DbContext) == RoleEnum.Admin)
            {
                IsOrgVisible = true;
            }
            else
            {
                SelectedOrg = App.CurrentUser.GetOrg(App.DbContext);
            }
        }

        async private void OnSaveCommand()
        {
            if (!Checks())
            {
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Ошибка", "ФИО, логин, пароли, телефон, роль - обязательные поля для заполнения! Также проверьте, что пароли совпадают!",
                    ButtonEnum.Ok);

                await box.ShowAsync();
                return;
            }

            bool isAdd = false;
            if (_currentUser == null)
            {
                _currentUser = new User();
                isAdd = true;
            }

            _currentUser.FullName = FullName;
            _currentUser.Login = Login;
            _currentUser.Password = Password;
            _currentUser.PhoneNumber = PhoneNumber;
            _currentUser.OrganizationId = SelectedOrg.Id;
            _currentUser.RoleId = SelectedRole.Id;

            if (isAdd)
            {
                App.DbContext.Users.Add(_currentUser);
            }
            else
            {
                App.DbContext.Entry(_currentUser).State = EntityState.Modified;
            }
            await App.DbContext.SaveChangesAsync();

            var box2 = MessageBoxManager
                    .GetMessageBoxStandard("Успешно", "Пользователь сохранен!",
                    ButtonEnum.Ok);
            await box2.ShowAsync();

            (_usersView.DataContext as AllUsersViewModel).UpdateUsers();
            // and go back
            OnCancelCommand();
        }

        private void OnCancelCommand()
        {
            App.CurrentWindowViewModel.ChangeView(_usersView);
        }

        private bool Checks()
        {
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
                return false;
            if (Password != PasswordAgain)
                return false;
            if (string.IsNullOrWhiteSpace(PhoneNumber))
                return false;
            if (SelectedRole == null || SelectedOrg == null)
                return false;

            var theUser = App.DbContext.Users.ToList().FirstOrDefault(x => x.FullName == FullName || x.PhoneNumber == PhoneNumber);
            if (theUser != null)
            {
                return false;
            }

            return true;
        }

        private User _currentUser;
        private AllUsersView _usersView;
        
        [Reactive]
        public bool IsErrorTextVisible { get; set; }
        [Reactive]
        public bool IsOrgVisible { get; set; }
        [Reactive]
        public string FullName { get; set; }
        [Reactive]
        public string Login { get; set; }
        [Reactive]
        public string Password { get; set; }
        [Reactive]
        public string PasswordAgain { get; set; }
        [Reactive]
        public string PhoneNumber { get; set; }

        [Reactive]
        public Role SelectedRole { get; set; }
        [Reactive]
        public Organization SelectedOrg { get; set; }

        public ObservableCollection<Role> AllRoles { get; set; } = new ObservableCollection<Role>();
        public ObservableCollection<Organization> AllOrgs { get; set; } = new ObservableCollection<Organization>();

        [Reactive]
        public ICommand SaveCommand { get; set; }
        [Reactive]
        public ICommand CancelCommand { get; set; }
    }
}

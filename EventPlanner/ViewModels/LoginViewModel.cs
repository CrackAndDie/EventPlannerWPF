using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace EventPlanner.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        public LoginViewModel()
        {
            LoginCommand = ReactiveCommand.Create(OnLoginCommand);
        }

        async private void OnLoginCommand()
        {
            var users = App.DbContext.Users;
            var theUser = await users.FirstOrDefaultAsync(x => x.Login == Login && x.Password == Password);
            if (theUser == null)
            {
                IsErrorTextVisible = true;
                return;
            }
            IsErrorTextVisible = false;

            App.CurrentUser = theUser;
            App.CurrentWindowViewModel.ChangeView(new MainView());
        }

        [Reactive]
        public ICommand LoginCommand { get; set; }
        [Reactive]
        public bool IsErrorTextVisible { get; set; }
        [Reactive]
        public string Login { get; set; }
        [Reactive]
        public string Password { get; set; }
    }
}

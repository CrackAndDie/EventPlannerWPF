using ReactiveUI.Fody.Helpers;

namespace EventPlanner.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel() 
        {
            var theUser = App.CurrentUser;
            UserName = $"ФИО: {theUser.FullName}";
            UserRole = $"Роль: {theUser.GetRole(App.DbContext).Name}";
            UserOrg  = $"Организация: {theUser.GetOrg(App.DbContext).Name}";
        }

        [Reactive]
        public string UserName { get; set; }
        [Reactive]
        public string UserRole { get; set; }
        [Reactive]
        public string UserOrg { get; set; }
    }
}

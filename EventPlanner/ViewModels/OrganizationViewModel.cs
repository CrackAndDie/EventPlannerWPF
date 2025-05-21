using Microsoft.EntityFrameworkCore;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Windows.Input;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;

namespace EventPlanner.ViewModels
{
    public class OrganizationViewModel : ViewModelBase
    {
        public OrganizationViewModel()
        {
            AddCommand = ReactiveCommand.Create(OnAddCommand);
            CancelCommand = ReactiveCommand.Create(OnCancelCommand);
        }

        async private void OnAddCommand()
        {
            var orgs = App.DbContext.Organizations;
            var theOrg = await orgs.FirstOrDefaultAsync(x => x.Name == Name);
            if (theOrg != null)
            {
                IsErrorTextVisible = true;
                return;
            }
            IsErrorTextVisible = false;

            theOrg = new Entities.Organization() { Name = Name };
            App.DbContext.Organizations.Add(theOrg);

            var box = MessageBoxManager
                    .GetMessageBoxStandard("Успешно", "Организация добавлена!",
                    ButtonEnum.Ok);

            await box.ShowAsync();

            // go back
            OnCancelCommand();
        }

        private void OnCancelCommand()
        {
            App.CurrentWindowViewModel.ChangeView(new MainView());
        }

        [Reactive]
        public ICommand AddCommand { get; set; }
        [Reactive]
        public ICommand CancelCommand { get; set; }
        [Reactive]
        public bool IsErrorTextVisible { get; set; }
        [Reactive]
        public string Name { get; set; }
    }
}

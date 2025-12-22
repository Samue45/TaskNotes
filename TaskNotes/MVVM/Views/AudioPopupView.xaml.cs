using CommunityToolkit.Maui.Views;
using TaskNotes.MVVM.ViewModels;

namespace TaskNotes.MVVM.Views;

public partial class AudioPopupView : Popup
{
    public AudioPopupView(TaskGestorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        this.BackgroundColor = Colors.Transparent;

    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        var vm = BindingContext as TaskGestorViewModel;

        if (vm != null)
        {
            
            if (vm.IsListening)
            {
                
                if (vm.StopListeningCommand.CanExecute(null))
                    vm.StopListeningCommand.Execute(null);
            }

           
            if (!string.IsNullOrWhiteSpace(vm.NewTaskTitle))
            {
                if (vm.AddTaskCommand.CanExecute(null))
                {
                    
                    vm.AddTaskCommand.Execute(null);
                }
            }
        }

        await CloseAsync();
    }
}
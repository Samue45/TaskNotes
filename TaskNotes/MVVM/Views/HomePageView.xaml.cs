using CommunityToolkit.Maui.Extensions;
using TaskNotes.MVVM.ViewModels;


namespace TaskNotes.MVVM.Views;

public partial class HomePageView : ContentPage
{

    public HomePageView(TaskGestorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnAddTaskClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddTaskView((TaskGestorViewModel)this.BindingContext));
    }

    private void OnShowAudioPopupClicked(object sender, EventArgs e)
    {
        var popup = new AudioPopupView((TaskGestorViewModel)this.BindingContext);

        this.ShowPopup(popup);
    }
}


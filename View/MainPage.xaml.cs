using lms.android.ViewModel;

namespace lms.android.View;

public partial class MainPage : ContentPage
{
    private readonly LMSViewModel _viewModel;

    public MainPage(LMSViewModel viewModel)
    {
        InitializeComponent();
        // Set the binding context to your course view model
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Automatically fetch courses as soon as the main screen opens
        if (_viewModel.GetCourseCommand.CanExecute(null))
        {
            await _viewModel.GetCourseCommand.ExecuteAsync(null);
        }
    }
}

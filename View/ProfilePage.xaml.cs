using lms.android.ViewModel;

namespace lms.android.View;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfilePage()
    {
        InitializeComponent();
        _viewModel = new ProfileViewModel();
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        int userId = Preferences.Default.Get("CurrentUserID", 0);
        if (_viewModel.LoadProfileCommand.CanExecute(userId))
        {
            _viewModel.LoadProfileCommand.Execute(userId);
        }
    }
}
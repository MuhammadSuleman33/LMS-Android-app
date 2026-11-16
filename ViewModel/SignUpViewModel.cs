using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using lms.android.Services;

namespace lms.android.ViewModel;

public partial class SignUpViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _databaseService;
    private string _fullName;
    private string _username;
    private string _email;
    private string _phone;
    private string _password;
    private string _confirmPassword;
    private bool _isBusy;
    private ICommand _goToLoginCommand; 

    public SignUpViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        SignUpCommand = new Command(async () => await ExecuteSignUpCommandAsync(), () => IsNotBusy);
    }

    public string FullName { get => _fullName; set { _fullName = value; OnPropertyChanged(); } }
    public string Username { get => _username; set { _username = value; OnPropertyChanged(); } }
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
    public string Phone { get => _phone; set { _phone = value; OnPropertyChanged(); } }
    public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }
    public string ConfirmPassword { get => _confirmPassword; set { _confirmPassword = value; OnPropertyChanged(); } }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotBusy));
            ((Command)SignUpCommand).ChangeCanExecute();
        }
    }

    public bool IsNotBusy => !IsBusy;

    public ICommand SignUpCommand { get; }

    
    public ICommand GoToLoginCommand
    {
        get => _goToLoginCommand ??= new Command(async () => await ExecuteGoToLoginCommandAsync());
    }

    private async Task ExecuteSignUpCommandAsync()
    {
        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Username) ||
            string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Shell.Current.DisplayAlert("Validation Error", "All fields except Phone are required.", "OK");
            return;
        }

        if (Password != ConfirmPassword)
        {
            await Shell.Current.DisplayAlert("Validation Error", "Passwords do not match.", "OK");
            return;
        }

        IsBusy = true;
        var (success, message) = await _databaseService.RegisterUserAsync(Username, Email, Phone, Password, FullName);
        IsBusy = false;

        if (success)
        {
            await Shell.Current.DisplayAlert("Success", message, "OK");
            await ExecuteGoToLoginCommandAsync(); 
        }
        else
        {
            await Shell.Current.DisplayAlert("Registration Failed", message, "OK");
        }
    }

    
    private async Task ExecuteGoToLoginCommandAsync()
    {
        try
        {
            if (Shell.Current.Navigation.ModalStack.Count > 0)
            {
                await Shell.Current.Navigation.PopModalAsync();
            }
            else
            { 
                await Shell.Current.GoToAsync("..");   
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Navigation Error", ex.Message, "OK");
        }
    }
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using lms.android.Services;
using lms.android.View;

namespace lms.android.ViewModel;

public partial class LoginViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _databaseService;
    private string _usernameOrEmail = string.Empty;
    private string _password = string.Empty;
    private string _selectedRole = string.Empty;
    private bool _isBusy;

    public LoginViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;

        // Populate the 5 required roles for the login screen Picker
        AvailableRoles = new ObservableCollection<string>
        {
            "Student",
            "Teacher",
            "HOD",
            "Competent Authority",
            "Super Admin"
        };

        LoginCommand = new Command(async () => await ExecuteLoginCommandAsync(), () => IsNotBusy);

        // Use Shell routing instead of PushModalAsync to prevent D_FAULT_SignUpPage route crashes
        GoToForgotPasswordCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(ForgotPasswordPage)));

        GoToSignUpCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(SignUpPage)));
    }

    public ObservableCollection<string> AvailableRoles { get; }

    public string UsernameOrEmail
    {
        get => _usernameOrEmail;
        set { _usernameOrEmail = value; OnPropertyChanged(); }
    }

    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    public string SelectedRole
    {
        get => _selectedRole;
        set { _selectedRole = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotBusy));
            ((Command)LoginCommand).ChangeCanExecute();
        }
    }

    public bool IsNotBusy => !IsBusy;

    public ICommand LoginCommand { get; }
    public ICommand GoToForgotPasswordCommand { get; set; }
    public ICommand GoToSignUpCommand { get; set; }

    private async Task ExecuteLoginCommandAsync()
    {
        // Validate all 3 fields including the selected role
        if (string.IsNullOrWhiteSpace(UsernameOrEmail) ||
            string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(SelectedRole))
        {
            await Shell.Current.DisplayAlert("Validation Error", "Please enter your credentials and select a role.", "OK");
            return;
        }

        IsBusy = true;

        try
        {
            string usernameInput = UsernameOrEmail.Trim();
            string passwordInput = Password;
            string roleInput = SelectedRole;

            // Run SQL connections on a background thread so Android UI never freezes (prevents ANR)
            var user = await Task.Run(async () =>
                await _databaseService.GetUserForLoginAsync(usernameInput, roleInput));

            if (user == null)
            {
                await Shell.Current.DisplayAlert("Login Failed", "Username or Email does not exist, or account is inactive.", "OK");
                return;
            }

            if (user.PasswordHash != passwordInput)
            {
                await Shell.Current.DisplayAlert("Login Failed", "Invalid password.", "OK");
                return;
            }

            // Role lock check
            if (!string.IsNullOrWhiteSpace(user.LockedRoleName) &&
                !string.Equals(user.LockedRoleName, roleInput, StringComparison.OrdinalIgnoreCase))
            {
                await Shell.Current.DisplayAlert(
                    "Access Denied",
                    $"This account is registered as {user.LockedRoleName} and cannot be used to log in as {roleInput}.",
                    "OK");
                return;
            }

            // Lock account to selected role on first login & update last login timestamp on background thread
            await Task.Run(async () =>
            {
                await _databaseService.LockUserRoleAsync(user.UserID, roleInput);
                await _databaseService.UpdateLastLoginAsync(user.UserID);
            });

            bool isVerified = user.VerificationStatus is "Verified" or "Approved";
            string effectiveRole = isVerified && !string.IsNullOrWhiteSpace(user.RoleName)
                ? user.RoleName
                : roleInput;

            // Save session state + 15-minute token expiry + unlock AppShell tabs
            Preferences.Default.Set("CurrentUserRole", isVerified ? effectiveRole : "");
            AppShell.StartUserSession(user.UserID, user.FullName, effectiveRole, user.VerificationStatus);

            // Clear password field from memory
            Password = string.Empty;

            // Navigate to MainPage if verified, or ProfilePage if still pending/rejected
            await Shell.Current.GoToAsync(isVerified ? "//MainPage" : "//ProfilePage");
        }
        catch (UnauthorizedAccessException ex)
        {
            await Shell.Current.DisplayAlert("Access Denied", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Database Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
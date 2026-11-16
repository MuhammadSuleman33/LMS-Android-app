using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using lms.android.Services;

namespace lms.android.ViewModel;

[QueryProperty(nameof(TargetEmail), "Email")]
public partial class ResetPasswordViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _databaseService;
    private string _targetEmail;
    private string _otpInput;
    private string _newPassword;
    private string _confirmNewPassword;
    private bool _isBusy;

    public ResetPasswordViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        ResetPasswordCommand = new Command(async () => await ExecuteResetPasswordCommandAsync(), () => IsNotBusy);
    }

    public string TargetEmail { get => _targetEmail; set { _targetEmail = value; OnPropertyChanged(); } }
    public string OtpInput { get => _otpInput; set { _otpInput = value; OnPropertyChanged(); } }
    public string NewPassword { get => _newPassword; set { _newPassword = value; OnPropertyChanged(); } }
    public string ConfirmNewPassword { get => _confirmNewPassword; set { _confirmNewPassword = value; OnPropertyChanged(); } }
    public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotBusy)); ((Command)ResetPasswordCommand).ChangeCanExecute(); } }
    public bool IsNotBusy => !IsBusy;

    public ICommand ResetPasswordCommand { get; }

    private async Task ExecuteResetPasswordCommandAsync()
    {
        if (string.IsNullOrWhiteSpace(OtpInput) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmNewPassword))
        {
            await Shell.Current.DisplayAlert("Validation Error", "All fields are required.", "OK");
            return;
        }

        if (NewPassword != ConfirmNewPassword)
        {
            await Shell.Current.DisplayAlert("Validation Error", "New passwords do not match.", "OK");
            return;
        }

        IsBusy = true;

        var activeToken = await _databaseService.GetActiveResetTokenAsync(TargetEmail);
        if (activeToken == null)
        {
            IsBusy = false;
            await Shell.Current.DisplayAlert("Error", "No valid transaction sequence or verification token expired.", "OK");
            return;
        }

        if (activeToken.Value.OTPHash != OtpInput)
        {
            await _databaseService.AddResetAttemptAsync(activeToken.Value.ResetTokenID);
            IsBusy = false;
            await Shell.Current.DisplayAlert("Verification Failed", "Incorrect validation OTP input code.", "OK");
            return;
        }

        var (success, message) = await _databaseService.CompletePasswordResetAsync(activeToken.Value.ResetTokenID, NewPassword);
        IsBusy = false;

        if (success)
        {
            await Shell.Current.DisplayAlert("Success", "Password updated successfully!", "OK");
            await Shell.Current.GoToAsync("///LoginPage");
        }
        else
        {
            await Shell.Current.DisplayAlert("Failed", message, "OK");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

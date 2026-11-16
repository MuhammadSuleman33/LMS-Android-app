using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using lms.android.Services;
using lms.android.View;

namespace lms.android.ViewModel;

public partial class ForgotPasswordViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _databaseService;
    private readonly EmailService _emailService;
    private string _email;
    private bool _isBusy;

    public ForgotPasswordViewModel(DatabaseService databaseService, EmailService emailService)
    {
        _databaseService = databaseService;
        _emailService = emailService;
        SendOtpCommand = new Command(async () => await ExecuteSendOtpCommandAsync(), () => IsNotBusy);
        BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    public ICommand BackCommand { get; }
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
    public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotBusy)); ((Command)SendOtpCommand).ChangeCanExecute(); } }
    public bool IsNotBusy => !IsBusy;

    public ICommand SendOtpCommand { get; }

    private async Task ExecuteSendOtpCommandAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            await Shell.Current.DisplayAlert("Validation Error", "Please enter your email address.", "OK");
            return;
        }

        IsBusy = true;
        string plainOtp = new Random().Next(100000, 999999).ToString();

        var tokenInfo = await _databaseService.UpdatePasswordResetTokensAsync(Email, plainOtp);
        IsBusy = false;

        if (tokenInfo != null)
        {
            bool isEmailSent = await _emailService.SendOtpEmailAsync(Email, plainOtp);

            // Explicitly pass email along context path queries forward
            await Shell.Current.GoToAsync($"{nameof(ResetPasswordPage)}?Email={Email}");
        }
        else
        {
            await Shell.Current.DisplayAlert("Error", "Email address could not be validated.", "OK");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

using lms.android.View;

namespace lms.android;

public partial class AppShell : Shell
{
    private static readonly TimeSpan SessionTimeout = TimeSpan.FromMinutes(15);
    private IDispatcherTimer? _sessionTimer;

    public AppShell()
    {
        InitializeComponent();

        // Register sub-pages that are NOT declared inside AppShell.xaml
        Routing.RegisterRoute(nameof(DetailsPage), typeof(DetailsPage));
        Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));
        Routing.RegisterRoute(nameof(ResetPasswordPage), typeof(ResetPasswordPage));
        Routing.RegisterRoute(nameof(SignUpPage), typeof(SignUpPage));

        ApplyAccessRules();
        StartSessionMonitor();

        // Check if session is active on startup; if expired or logged out, go straight to LoginPage
        Dispatcher.Dispatch(async () => await ValidateSessionOrRedirectAsync());
    }

    public void ApplyAccessRules()
    {
        bool isLoggedIn = IsSessionValid();
        bool verified = isLoggedIn &&
                        Preferences.Default.Get("CurrentUserVerificationStatus", "Pending")
                        is "Verified" or "Approved";

        // Keep ProfileShellContent always visible so //ProfilePage route never fails;
        // only show Courses tab once verified!
        if (CoursesShellContent != null)
        {
            CoursesShellContent.IsVisible = verified;
        }

        if (ProfileShellContent != null)
        {
            ProfileShellContent.IsVisible = true;
        }
    }

    public bool IsSessionValid()
    {
        int userId = Preferences.Default.Get("CurrentUserID", 0);
        if (userId <= 0)
            return false;

        long expiryTicks = Preferences.Default.Get("TokenExpiryTicks", 0L);

        if (expiryTicks > 0 && DateTime.UtcNow.Ticks > expiryTicks)
            return false;

        return true;
    }

    public static void StartUserSession(int userId, string fullName, string roleName, string verificationStatus)
    {
        Preferences.Default.Set("CurrentUserID", userId);
        Preferences.Default.Set("CurrentUserFullName", fullName);
        Preferences.Default.Set("RequestedRole", roleName);
        Preferences.Default.Set("CurrentUserVerificationStatus", verificationStatus);
        Preferences.Default.Set("TokenExpiryTicks", DateTime.UtcNow.Add(SessionTimeout).Ticks);

        if (Current is AppShell shell)
        {
            shell.ApplyAccessRules();
        }
    }

    public async Task ValidateSessionOrRedirectAsync()
    {
        if (!IsSessionValid())
        {
            await LogoutAsync();
        }
        else
        {
            ApplyAccessRules();
        }
    }

    public async Task LogoutAsync()
    {
        Preferences.Default.Remove("CurrentUserID");
        Preferences.Default.Remove("CurrentUserFullName");
        Preferences.Default.Remove("RequestedRole");
        Preferences.Default.Remove("CurrentUserVerificationStatus");
        Preferences.Default.Remove("TokenExpiryTicks");
        Preferences.Default.Remove("AuthToken");

        ApplyAccessRules();

        if (Current != null)
        {
            await Current.GoToAsync("//LoginPage");
        }
    }

    private void StartSessionMonitor()
    {
        _sessionTimer = Dispatcher.CreateTimer();
        _sessionTimer.Interval = TimeSpan.FromSeconds(30);
        _sessionTimer.Tick += async (s, e) =>
        {
            int userId = Preferences.Default.Get("CurrentUserID", 0);
            if (userId > 0 && !IsSessionValid())
            {
                await LogoutAsync();
            }
        };
        _sessionTimer.Start();
    }
}
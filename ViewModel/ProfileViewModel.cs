using System.Collections.ObjectModel;
using System.Windows.Input;
using lms.android.Model;
using lms.android.Services;

namespace lms.android.ViewModel;

public class ProfileViewModel : BaseViewModel
{
    private const string StatusKey = "CurrentUserVerificationStatus";
    private const string RoleKey = "RequestedRole";

    private readonly StudentProfileService profileService;
    private readonly DatabaseService databaseService;
    private StudentProfile profile = new();
    private TeacherProfileModel _teacherProfile = new();
    private HODProfileModel _hodProfile = new();
    private CompetentAuthorityProfileModel _competentAuthorityProfile = new();
    private SuperAdminProfileModel _superAdminProfile = new(); // <--- ADDED NEW LINE
    private UserAccountProfile _accountProfile = new();
    private string _errorMessage = string.Empty;
    private string _verificationStatus;
    private string _currentRole;
    private bool _isBusy;

    // <--- ADDED NEW BLOCK: Super Admin Profile Property
    public SuperAdminProfileModel SuperAdminProfile
    {
        get => _superAdminProfile;
        set
        {
            _superAdminProfile = value;
            OnPropertyChanged();
        }
    }

    public CompetentAuthorityProfileModel CompetentAuthorityProfile
    {
        get => _competentAuthorityProfile;
        set
        {
            _competentAuthorityProfile = value;
            OnPropertyChanged();
        }
    }

    public HODProfileModel HODProfile
    {
        get => _hodProfile;
        set
        {
            _hodProfile = value;
            OnPropertyChanged();
        }
    }

    public TeacherProfileModel TeacherProfile
    {
        get => _teacherProfile;
        set
        {
            _teacherProfile = value;
            OnPropertyChanged();
        }
    }

    public StudentProfile Profile
    {
        get => profile;
        set
        {
            profile = value;
            OnPropertyChanged();
        }
    }

    public UserAccountProfile AccountProfile
    {
        get => _accountProfile;
        set
        {
            _accountProfile = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<PendingVerificationItem> PendingReviews { get; } = new();

    // ---------- Role & Verification State ----------

    public string CurrentRole
    {
        get => _currentRole;
        private set
        {
            _currentRole = value;
            OnPropertyChanged();
            RaiseRoleFlagsChanged();
        }
    }

    public string VerificationStatus
    {
        get => _verificationStatus;
        private set
        {
            _verificationStatus = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsVerified));
            OnPropertyChanged(nameof(IsNotVerified));
            OnPropertyChanged(nameof(IsPending));
            OnPropertyChanged(nameof(IsRejected));
            OnPropertyChanged(nameof(StatusTitle));
            OnPropertyChanged(nameof(StatusMessage));
            RaiseRoleFlagsChanged();
        }
    }

    private void RaiseRoleFlagsChanged()
    {
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(RoleIcon));
        OnPropertyChanged(nameof(IsStudent));
        OnPropertyChanged(nameof(IsTeacher));
        OnPropertyChanged(nameof(IsHOD));
        OnPropertyChanged(nameof(IsCompetentAuthority));
        OnPropertyChanged(nameof(IsSuperAdmin));
        OnPropertyChanged(nameof(IsStaffOrAdmin));
        OnPropertyChanged(nameof(HasPendingReviews));
    }

    // Verified and Approved both unlock role content
    public bool IsVerified => VerificationStatus is "Verified" or "Approved";
    public bool IsNotVerified => !IsVerified;
    public bool IsRejected => VerificationStatus == "Rejected";
    public bool IsPending => IsNotVerified && !IsRejected;

    // 5 Role Visibility Flags (Only true once Verified/Approved)
    public bool IsStudent => IsVerified && string.Equals(CurrentRole, "Student", StringComparison.OrdinalIgnoreCase);
    public bool IsTeacher => IsVerified && string.Equals(CurrentRole, "Teacher", StringComparison.OrdinalIgnoreCase);
    public bool IsHOD => IsVerified && string.Equals(CurrentRole, "HOD", StringComparison.OrdinalIgnoreCase);
    public bool IsCompetentAuthority => IsVerified && string.Equals(CurrentRole, "Competent Authority", StringComparison.OrdinalIgnoreCase);
    public bool IsSuperAdmin => IsVerified && string.Equals(CurrentRole, "Super Admin", StringComparison.OrdinalIgnoreCase);

    public bool IsStaffOrAdmin => IsTeacher || IsHOD || IsCompetentAuthority || IsSuperAdmin;
    public bool HasPendingReviews => IsStaffOrAdmin && PendingReviews.Count > 0;

    public string PageTitle => string.IsNullOrWhiteSpace(CurrentRole)
        ? "User Profile"
        : $"{CurrentRole} Profile";

    public string RoleIcon => CurrentRole switch
    {
        "Student" => "🎓",
        "Teacher" => "👨‍🏫",
        "HOD" => "🏛️",
        "Competent Authority" => "⚖️",
        "Super Admin" => "🛡️",
        _ => "👤"
    };

    public string StatusTitle => IsRejected ? "Verification Rejected" : "Verification Pending";

    public string StatusMessage => IsRejected
        ? "Your account verification was rejected. Please contact your administration."
        : "Your account is awaiting review. Role-related content will appear here once it is verified.";

    public string DisplayName => !string.IsNullOrWhiteSpace(AccountProfile?.FullName)
        ? AccountProfile.FullName
        : Preferences.Default.Get("CurrentUserFullName", string.Empty);

    public string RequestedRole => !string.IsNullOrWhiteSpace(CurrentRole)
        ? CurrentRole
        : Preferences.Default.Get(RoleKey, string.Empty);

    public new bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();

            ((Command)LoadProfileCommand)?.ChangeCanExecute();
            ((Command)UpdateCoreCommand)?.ChangeCanExecute();
            ((Command)UpdateContactCommand)?.ChangeCanExecute();
            ((Command)UpdateGuardianCommand)?.ChangeCanExecute();
            ((Command)UpdateTeacherPersonalCommand)?.ChangeCanExecute();
            ((Command)UpdateTeacherAcademicCommand)?.ChangeCanExecute();
            ((Command)UpdateTeacherExperienceCommand)?.ChangeCanExecute();
            ((Command)UpdateHODProfileCommand)?.ChangeCanExecute();
            ((Command)UpdateCompetentAuthorityCommand)?.ChangeCanExecute();
            ((Command)UpdateSuperAdminCommand)?.ChangeCanExecute(); // <--- ADDED NEW LINE
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadProfileCommand { get; }
    public ICommand UpdateCoreCommand { get; }
    public ICommand UpdateContactCommand { get; }
    public ICommand UpdateGuardianCommand { get; }
    public ICommand RefreshVerificationCommand { get; }
    public ICommand ApproveProfileCommand { get; }
    public ICommand RejectProfileCommand { get; }
    public ICommand LogoutCommand { get; }

    public ICommand UpdateTeacherPersonalCommand { get; }
    public ICommand UpdateTeacherAcademicCommand { get; }
    public ICommand UpdateTeacherExperienceCommand { get; }
    public ICommand UpdateHODProfileCommand { get; }
    public ICommand UpdateCompetentAuthorityCommand { get; }
    public ICommand UpdateSuperAdminCommand { get; } // <--- ADDED NEW LINE

    public ProfileViewModel()
    {
        profileService = new StudentProfileService();
        databaseService = new DatabaseService();
        Profile = new StudentProfile();
        TeacherProfile = new TeacherProfileModel();
        HODProfile = new HODProfileModel();
        CompetentAuthorityProfile = new CompetentAuthorityProfileModel();
        SuperAdminProfile = new SuperAdminProfileModel(); // <--- ADDED NEW LINE

        _verificationStatus = Preferences.Default.Get(StatusKey, "Pending");
        _currentRole = Preferences.Default.Get(RoleKey, "Student");

        LoadProfileCommand = new Command<int>(async (id) => await LoadProfileAsync(id), (_) => !IsBusy);
        UpdateCoreCommand = new Command(async () => await UpdateSectionAsync("UPDATE_CORE"), () => !IsBusy);
        UpdateContactCommand = new Command(async () => await UpdateSectionAsync("UPSERT_CONTACT"), () => !IsBusy);
        UpdateGuardianCommand = new Command(async () => await UpdateSectionAsync("UPSERT_GUARDIAN"), () => !IsBusy);
        RefreshVerificationCommand = new Command(async () => await CheckStatusAsync());

        UpdateTeacherPersonalCommand = new Command(async () => await UpdateTeacherSectionAsync("UPDATE_PERSONAL"), () => !IsBusy);
        UpdateTeacherAcademicCommand = new Command(async () => await UpdateTeacherSectionAsync("UPSERT_ACADEMIC"), () => !IsBusy);
        UpdateTeacherExperienceCommand = new Command(async () => await UpdateTeacherSectionAsync("UPSERT_EXPERIENCE"), () => !IsBusy);

        UpdateHODProfileCommand = new Command(async () => await UpdateHODSectionAsync(), () => !IsBusy);
        UpdateCompetentAuthorityCommand = new Command(async () => await UpdateCompetentAuthoritySectionAsync(), () => !IsBusy);
        UpdateSuperAdminCommand = new Command(async () => await UpdateSuperAdminSectionAsync(), () => !IsBusy); // <--- ADDED NEW LINE

        ApproveProfileCommand = new Command<PendingVerificationItem>(async (item) => await ReviewProfileAsync(item, "Approved"));
        RejectProfileCommand = new Command<PendingVerificationItem>(async (item) => await ReviewProfileAsync(item, "Rejected"));

        LogoutCommand = new Command(async () => await ExecuteLogoutAsync());
    }

    private async Task ExecuteLogoutAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Log Out",
            "Are you sure you want to log out of your account?",
            "Yes, Logout",
            "Cancel");

        if (!confirm)
            return;

        // Reset local ViewModel state
        Profile = new StudentProfile();
        TeacherProfile = new TeacherProfileModel();
        HODProfile = new HODProfileModel();
        CompetentAuthorityProfile = new CompetentAuthorityProfileModel();
        SuperAdminProfile = new SuperAdminProfileModel(); // <--- ADDED NEW LINE
        AccountProfile = new UserAccountProfile();
        PendingReviews.Clear();
        _verificationStatus = "Pending";
        _currentRole = string.Empty;

        // Clear Preferences, lock tabs, and navigate to //LoginPage
        if (Shell.Current is AppShell appShell)
        {
            await appShell.LogoutAsync();
        }
        else
        {
            Preferences.Default.Clear();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }

    private async Task CheckStatusAsync()
    {
        if (IsBusy) return;

        var userId = Preferences.Default.Get("CurrentUserID", 0);
        if (userId <= 0)
        {
            await ExecuteLogoutAsync();
            return;
        }

        await LoadProfileAsync(userId);

        if (!IsVerified)
        {
            var message = IsRejected
                ? "Your verification was rejected. Please contact your administration."
                : "Your account is still pending verification. Please check again later.";
            await Shell.Current.DisplayAlert(StatusTitle, message, "OK");
        }
    }

    public async Task RefreshVerificationAsync(int userId)
    {
        var acct = await Task.Run(async () => await databaseService.GetUserAccountProfileAsync(userId));
        if (acct != null)
        {
            AccountProfile = acct;

            Preferences.Default.Set(StatusKey, acct.VerificationStatus);
            if (!string.IsNullOrWhiteSpace(acct.RoleName) && acct.RoleName != "Unassigned")
            {
                Preferences.Default.Set(RoleKey, acct.RoleName);
                CurrentRole = acct.RoleName;
            }
            if (!string.IsNullOrWhiteSpace(acct.FullName))
            {
                Preferences.Default.Set("CurrentUserFullName", acct.FullName);
            }

            VerificationStatus = acct.VerificationStatus;
        }
        else
        {
            var status = await Task.Run(async () => await databaseService.GetVerificationStatusAsync(userId));
            Preferences.Default.Set(StatusKey, status);
            VerificationStatus = status;
            CurrentRole = Preferences.Default.Get(RoleKey, "Student");
        }

        // Immediately unlock/lock AppShell navigation items based on new verification status
        if (Shell.Current is AppShell appShell)
        {
            appShell.ApplyAccessRules();
        }

        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(RequestedRole));
    }

    private async Task LoadProfileAsync(int userId)
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            if (userId <= 0)
                userId = Preferences.Default.Get("CurrentUserID", 0);

            if (userId <= 0)
                return;

            await RefreshVerificationAsync(userId);
            if (!IsVerified)
                return;

            // 1. If the user is a Student, load their full StudentProfile
            // 1. If the user is a Student, load their full StudentProfile
            if (IsStudent)
            {
                var data = await Task.Run(async () => await profileService.GetStudentProfileAsync(userId));

                // Split AccountProfile.FullName into FirstName and LastName for fallback
                string acctFullName = AccountProfile?.FullName?.Trim() ?? string.Empty;
                string fallbackFirst = acctFullName;
                string fallbackLast = string.Empty;

                if (acctFullName.Contains(' '))
                {
                    int spaceIndex = acctFullName.IndexOf(' ');
                    fallbackFirst = acctFullName[..spaceIndex].Trim();
                    fallbackLast = acctFullName[(spaceIndex + 1)..].Trim();
                }

                if (data != null)
                {
                    // Fill any blank Student table fields from UserAccounts (AccountProfile)
                    if (data.StudentID <= 0)
                        data.StudentID = userId;

                    if (string.IsNullOrWhiteSpace(data.FirstName))
                        data.FirstName = fallbackFirst;

                    if (string.IsNullOrWhiteSpace(data.LastName))
                        data.LastName = fallbackLast;

                    if (string.IsNullOrWhiteSpace(data.FullName))
                        data.FullName = !string.IsNullOrWhiteSpace(acctFullName)
                            ? acctFullName
                            : $"{data.FirstName} {data.LastName}".Trim();

                    if (string.IsNullOrWhiteSpace(data.Email))
                        data.Email = AccountProfile?.Email ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(data.Phone))
                        data.Phone = AccountProfile?.Phone ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(data.RegistrationNo))
                        data.RegistrationNo = $"REG-{DateTime.Today.Year}-{userId:D4}";

                    Profile = data;
                }
                else
                {
                    // Fallback to basic account details if Student table row isn't created yet
                    Profile = new StudentProfile
                    {
                        StudentID = userId,
                        RegistrationNo = $"REG-{DateTime.Today.Year}-{userId:D4}",
                        FirstName = fallbackFirst,
                        LastName = fallbackLast,
                        FullName = acctFullName,
                        Email = AccountProfile?.Email ?? string.Empty,
                        Phone = AccountProfile?.Phone ?? string.Empty,
                        AcademicStatus = "Active",
                        ExpectedGraduationDate = DateTime.Today.AddYears(4)
                    };
                }
            }
            // 2. If the user is a Teacher, HOD, Competent Authority, or Super Admin
            else if (IsStaffOrAdmin)
            {
                if (IsTeacher)
                {
                    var teacherData = await Task.Run(async () => await databaseService.GetTeacherProfileAsync(userId));
                    if (teacherData != null)
                    {
                        TeacherProfile = teacherData;
                    }
                    else
                    {
                        TeacherProfile = new TeacherProfileModel
                        {
                            TeacherID = userId,
                            FirstName = AccountProfile.FullName,
                            Email = AccountProfile.Email,
                            Phone = AccountProfile.Phone
                        };
                    }
                }
                else if (IsHOD)
                {
                    var hodData = await Task.Run(async () => await databaseService.GetHODProfileAsync(userId));
                    if (hodData != null)
                    {
                        HODProfile = hodData;
                    }
                    else
                    {
                        HODProfile = new HODProfileModel
                        {
                            HODID = userId,
                            FullName = AccountProfile.FullName,
                            Email = AccountProfile.Email,
                            Phone = AccountProfile.Phone
                        };
                    }
                }
                else if (IsCompetentAuthority)
                {
                    var caData = await Task.Run(async () => await databaseService.GetCompetentAuthorityProfileAsync(userId));
                    if (caData != null)
                    {
                        CompetentAuthorityProfile = caData;
                    }
                    else
                    {
                        CompetentAuthorityProfile = new CompetentAuthorityProfileModel
                        {
                            AuthorityID = userId,
                            FullName = AccountProfile.FullName,
                            Email = AccountProfile.Email,
                            Phone = AccountProfile.Phone
                        };
                    }
                }
                // <--- Load Super Admin Profile from SQL
                else if (IsSuperAdmin)
                {
                    var saData = await Task.Run(async () => await databaseService.GetSuperAdminProfileAsync(userId));
                    if (saData != null)
                    {
                        SuperAdminProfile = saData;
                    }
                    else
                    {
                        SuperAdminProfile = new SuperAdminProfileModel
                        {
                            AdminID = userId,
                            FullName = AccountProfile.FullName,
                            Email = AccountProfile.Email,
                            Phone = AccountProfile.Phone
                        };
                    }
                }

                // Load Pending Approvals Queue for this reviewer role
                var pending = await Task.Run(async () => await databaseService.GetPendingProfilesForReviewerAsync(CurrentRole));
                PendingReviews.Clear();
                foreach (var item in pending)
                {
                    PendingReviews.Add(item);
                }
                OnPropertyChanged(nameof(HasPendingReviews));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error Loading Profile: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ReviewProfileAsync(PendingVerificationItem? item, string newStatus)
    {
        if (item == null || IsBusy) return;

        try
        {
            IsBusy = true;
            int reviewerId = Preferences.Default.Get("CurrentUserID", 0);

            var result = await Task.Run(async () =>
                await databaseService.ProcessProfileVerificationAsync(
                    item.TargetUserID,
                    item.ProfileType,
                    reviewerId,
                    CurrentRole,
                    newStatus,
                    $"{newStatus} by {CurrentRole}"));

            if (result.Success)
            {
                PendingReviews.Remove(item);
                OnPropertyChanged(nameof(HasPendingReviews));
                await Shell.Current.DisplayAlert("Success", result.Message, "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Authorization / Workflow Error", result.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UpdateSectionAsync(string action)
    {
        if (IsBusy || Profile == null || !IsVerified) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            int userId = Preferences.Default.Get("CurrentUserID", 0);
            if (Profile.StudentID <= 0)
                Profile.StudentID = userId;

            var result = await Task.Run(async () => await profileService.UpdateStudentProfileAsync(action, Profile));

            if (result.Success)
            {
                await RefreshVerificationAsync(userId);
                await Shell.Current.DisplayAlert("Success", result.Message, "OK");
            }
            else
            {
                ErrorMessage = result.Message;
                await Shell.Current.DisplayAlert("Error", result.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await Shell.Current.DisplayAlert("Exception", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
    private async Task UpdateTeacherSectionAsync(string action)
    {
        if (IsBusy || TeacherProfile == null || !IsVerified) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            if (TeacherProfile.TeacherID <= 0)
                TeacherProfile.TeacherID = Preferences.Default.Get("CurrentUserID", 0);

            var result = await Task.Run(async () => await databaseService.UpdateTeacherProfileAsync(action, TeacherProfile));
            if (result.Success)
            {
                await RefreshVerificationAsync(Preferences.Default.Get("CurrentUserID", 0));
                await Shell.Current.DisplayAlert("Success", result.Message, "OK");
            }
            else
            {
                ErrorMessage = result.Message;
                await Shell.Current.DisplayAlert("Error", result.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await Shell.Current.DisplayAlert("Exception", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UpdateHODSectionAsync()
    {
        if (IsBusy || HODProfile == null || !IsVerified) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            int userId = Preferences.Default.Get("CurrentUserID", 0);
            var result = await Task.Run(async () => await databaseService.UpdateHODProfileAsync(userId, HODProfile));

            if (result.Success)
            {
                await RefreshVerificationAsync(userId);
                await Shell.Current.DisplayAlert("Success", result.Message, "OK");
            }
            else
            {
                ErrorMessage = result.Message;
                await Shell.Current.DisplayAlert("Error", result.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await Shell.Current.DisplayAlert("Exception", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UpdateCompetentAuthoritySectionAsync()
    {
        if (IsBusy || CompetentAuthorityProfile == null || !IsVerified) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            int userId = Preferences.Default.Get("CurrentUserID", 0);
            var result = await Task.Run(async () => await databaseService.UpdateCompetentAuthorityProfileAsync(userId, CompetentAuthorityProfile));

            if (result.Success)
            {
                await RefreshVerificationAsync(userId);
                await Shell.Current.DisplayAlert("Success", result.Message, "OK");
            }
            else
            {
                ErrorMessage = result.Message;
                await Shell.Current.DisplayAlert("Error", result.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await Shell.Current.DisplayAlert("Exception", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // <--- ADDED NEW BLOCK: Save / Update Super Admin Profile in SQL
    private async Task UpdateSuperAdminSectionAsync()
    {
        if (IsBusy || SuperAdminProfile == null || !IsVerified) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            int userId = Preferences.Default.Get("CurrentUserID", 0);
            var result = await Task.Run(async () => await databaseService.UpdateSuperAdminProfileAsync(userId, SuperAdminProfile));

            if (result.Success)
            {
                await RefreshVerificationAsync(userId);
                await Shell.Current.DisplayAlert("Success", result.Message, "OK");
            }
            else
            {
                ErrorMessage = result.Message;
                await Shell.Current.DisplayAlert("Error", result.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await Shell.Current.DisplayAlert("Exception", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
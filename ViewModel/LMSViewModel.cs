using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using lms.android.Services;
using lms.android.View;

namespace lms.android.ViewModel;

public partial class LMSViewModel : BaseViewModel
{
    private readonly CourseServices courseServices;
    private readonly IConnectivity connectivity; // Fixed: Changed field to readonly style

    public ObservableCollection<Model.Course> Course { get; } = new();

    public LMSViewModel(CourseServices courseServices, IConnectivity connectivity)
    {
        Title = "LMS Courses";
        this.courseServices = courseServices;
        this.connectivity = connectivity;
    }

    [ObservableProperty]
    bool isRefreshing;

    [RelayCommand]
    private async Task GetCourseAsync()
    {
        if (IsBusy)
            return;

        try
        {
            if (connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlert("Internet Issue",
                    "Please check internet and try again.", "OK");
                return;
            }

            IsBusy = true;

            var courses = await courseServices.GetCourses();

            Course.Clear();
            foreach (var item in courses)
                Course.Add((Model.Course)item);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error!",
                $"Unable to get courses: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false; // Fixed: Capitalized to point to generated property observable wrapper
        }
    }

    [RelayCommand]
    private async Task GoToDetailsAsync(Model.Course course)
    {
        if (course is null)
            return;

        await Shell.Current.GoToAsync(nameof(DetailsPage), true,
            new Dictionary<string, object>
            {
                { "Course", course }
            });
    }

    // =========================================================================
    // NEW: Profile Page Navigation Command
    // =========================================================================
    [RelayCommand]
    private async Task NavigateToProfileAsync()
    {
        // Navigates directly over to the registered route string path name
        await Shell.Current.GoToAsync("ProfilePage");
    }
}

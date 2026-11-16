using CommunityToolkit.Mvvm.ComponentModel;
using lms.android.Model;
using lms.android.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace lms.android.ViewModel;

[QueryProperty(nameof(SelectedCourse), "Course")]
public partial class LMSDetailsViewModel : ObservableObject
{
    private readonly CourseServices courseServices;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private Model.Course selectedCourse;

    public ObservableCollection<Model.Course> Courses { get; } = new();

    public LMSDetailsViewModel(CourseServices courseServices)
    {
        Title = "Course Details";
        this.courseServices = courseServices;
    }

    public async Task GetCourseAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            var fetchedCourses = await courseServices.GetCourses();

            if (Courses.Count != 0)
                Courses.Clear();

            foreach (var item in fetchedCourses)
            {
                Courses.Add(item);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error!",
                $"Unable to get Courses: {ex.Message}.", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

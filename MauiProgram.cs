using CommunityToolkit.Maui;
using lms.android.Services;
using lms.android.View;
using lms.android.ViewModel;
using Microsoft.Extensions.Logging;

namespace lms.android;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
            builder
            .UseMauiApp<App>()
             .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Core services
        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<CourseServices>();
        builder.Services.AddSingleton<EmailService>();
        builder.Services.AddTransient<lms.android.Services.StudentProfileService>();


        

        // Authentication (View Models)
        builder.Services.AddTransient<LoginViewModel>(); 
        builder.Services.AddTransient<SignUpViewModel>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<ResetPasswordViewModel>();

        // Content  ViewModels
        builder.Services.AddSingleton<LMSViewModel>();
        builder.Services.AddTransient<LMSDetailsViewModel>();
        builder.Services.AddTransient<lms.android.ViewModel.ProfileViewModel>();


        // Authentication Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<SignUpPage>();
        builder.Services.AddTransient<ForgotPasswordPage>();
        builder.Services.AddTransient<ResetPasswordPage>();

        // Content pages
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<DetailsPage>();
        builder.Services.AddTransient<lms.android.View.ProfilePage>();
       



#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
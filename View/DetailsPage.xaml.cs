namespace lms.android.View;
 
public partial class DetailsPage : ContentPage
{
    public DetailsPage(LMSDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        
    }
}   
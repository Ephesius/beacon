using Beacon.ViewModels;

namespace Beacon;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Lock to portrait orientation as per spec
        if (DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.Android)
        {
            Microsoft.Maui.Controls.Application.Current!.MainPage!.Rotation = 0;
        }
    }
}
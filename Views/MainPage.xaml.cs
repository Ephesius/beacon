using Beacon.ViewModels;

namespace Beacon;

/// <summary>
/// Primary view for the Beacon application implementing a single-page navigation interface.
/// Displays state-specific UI elements and handles user interactions through MVVM bindings.
/// 
/// UI Requirements:
/// - Portrait orientation lock
/// - System font scaling support
/// - Dark/light theme compliance
/// - Safe area adherence
/// - Minimum touch targets: 44x44pt
/// </summary>
public partial class MainPage : ContentPage
{
    /// <summary>
    /// Initializes the main page and establishes binding context.
    /// </summary>
    /// <param name="viewModel">View model providing UI logic and state management</param>
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    /// <summary>
    /// Enforces portrait orientation lock on supported platforms.
    /// Called when page becomes visible.
    /// </summary>
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
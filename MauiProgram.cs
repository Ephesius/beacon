using Beacon.Services;
using Beacon.ViewModels;
using Beacon.Converters;
using Microsoft.Extensions.Logging;

namespace Beacon
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register services
            builder.Services.AddSingleton<IBeaconLocationService, BeaconLocationService>();
            builder.Services.AddSingleton<IStateService, StateService>();

            // Register MAUI dependencies required by the services
            builder.Services.AddSingleton(Geolocation.Default);
            builder.Services.AddSingleton(Compass.Default);

            // Register ViewModels
            builder.Services.AddSingleton<MainViewModel>();

            // Register Pages
            builder.Services.AddSingleton<MainPage>();

            RegisterConverters();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void RegisterConverters()
        {
            if (Application.Current?.Resources != null)
            {
                var resources = Application.Current.Resources;
                resources["SignalStatusToColorConverter"] = new SignalStatusToColorConverter();
                resources["StateToVisibilityConverter"] = new StateToVisibilityConverter();
                resources["SignalStatusToEnabledConverter"] = new SignalStatusToEnabledConverter();
            }
        }
    }
}

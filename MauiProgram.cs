using Beacon.Services;
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

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

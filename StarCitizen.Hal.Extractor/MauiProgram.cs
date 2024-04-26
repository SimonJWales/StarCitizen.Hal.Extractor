
using CommunityToolkit.Maui.Core;
using Hal.Extractor.Services;
using Microsoft.Extensions.Logging;
using StarCitizen.Hal.Extractor.Services;
using StarCitizen.Hal.Extractor.ViewModels;
using System.Runtime.Versioning;

namespace StarCitizen.Hal.Extractor
{
    [SupportedOSPlatform("windows")]
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkitCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif


            builder.Services.AddSingleton<AppState>();

            builder.Services.AddSingleton<FileService>();

            builder.Services.AddSingleton<ExtractionService>();

            builder.Services.AddSingleton<XMLCryExtraction>();

            builder.Services.AddSingleton<MainPageViewModel>();

            builder.Services.AddSingleton<MainPage>();

            builder.Services.AddSingleton<ILogger>(serviceProvider =>
            {
                AppState appState = serviceProvider.GetRequiredService<AppState>();

                return new Log(appState);
            });

            return builder.Build();
        }
    }
}

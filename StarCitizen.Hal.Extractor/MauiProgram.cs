
using CommunityToolkit.Maui.Core;
using Hal.Extractor.Services;
using Microsoft.Extensions.Logging;
using StarCitizen.Hal.Extractor.Services;
using StarCitizen.Hal.Extractor.ViewModels;

namespace StarCitizen.Hal.Extractor
{
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

            builder.Services.AddSingleton<ILogger>(serviceProvider =>
            {
                var logPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                return new Log(logPath);
            });

            builder.Services.AddSingleton<AppState>();

            builder.Services.AddSingleton<FileService>();

            builder.Services.AddSingleton<ExtractionService>();

            builder.Services.AddSingleton<XMLCryExtraction>();

            builder.Services.AddSingleton<MainPageViewModel>();

            builder.Services.AddSingleton<MainPage>();

            return builder.Build();
        }
    }
}

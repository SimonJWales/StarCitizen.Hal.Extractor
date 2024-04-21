using Hal.Extractor.Services;
using StarCitizen.Hal.Extractor.ViewModels;
using System.Reflection;

namespace StarCitizen.Hal.Extractor
{
    public partial class MainPage : ContentPage
    {
        AppState? AppState { get; set; }

        public MainPage(
            MainPageViewModel viewModel,
            AppState appState)
        {
            InitializeComponent();

            AppState = appState;

            BindingContext = viewModel;

            AppState!.FileCountHasChanged = () =>
            {
                viewModel.FilesExtracted = AppState.FileCount;
            };

            AppState!.ConvertedCountHasChanged = () =>
            {
                viewModel.FilesConverted = AppState.ConvertedCount;
            };

            AppState!.LogErrorStateHasChanged = () =>
            {
                if (AppState!.LogErrorState)
                {
                    viewModel.LogErrors = true;
                }

                if (!AppState!.LogErrorState)
                {
                    viewModel.LogErrors = false;
                }
            };
        }
    }
}

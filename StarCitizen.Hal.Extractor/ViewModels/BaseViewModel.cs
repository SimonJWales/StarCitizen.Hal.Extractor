
using CommunityToolkit.Mvvm.ComponentModel;

namespace StarCitizen.Hal.Extractor.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        string? title;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AreWeNotExtracting))]
        bool areWeExtracting;

        public bool AreWeNotExtracting => !AreWeExtracting;

        [ObservableProperty]
        bool areNewExtensionsFound;

        [ObservableProperty]
        string? extractFromPath;

        [ObservableProperty]
        string? extractToPath;

        [ObservableProperty]
        string? updateInfoText;

        [ObservableProperty]
        string? updateInfoTextColour;

        [ObservableProperty]
        string? extractionTimer;

        [ObservableProperty]
        int filesExtracted;

        [ObservableProperty]
        int filesConverted;

        [ObservableProperty]
        bool logErrors;

    }
}

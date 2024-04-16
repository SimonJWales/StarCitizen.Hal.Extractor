
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Input;
using Hal.Extractor.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace StarCitizen.Hal.Extractor.ViewModels
{
    public partial class MainPageViewModel : BaseViewModel
    {
        ExtractionService? ExtractionService { get; }

        XMLCryExtraction XMLCryExtraction { get; }

        public ObservableCollection<string>? ObservedFileTypes { get; } = [];

        public ObservableCollection<string>? ObservedExtensions { get; } = [];

        Stopwatch? _stopwatch;

        Timer? UiTimer { get; set; }

        public MainPageViewModel(
            ExtractionService extractionService,
            XMLCryExtraction xMLCryExtraction)
        {
            ExtractionService = extractionService;

            XMLCryExtraction = xMLCryExtraction;

            Title = Parameters.Title;

            Task.Run(async () =>
            {
                await GetExtensions(Parameters.AssetSeparator);
            });

            ExtractFromPath = FileService.ReadPreference(Parameters.ExtractFromRegistryName);

            ExtractToPath = FileService.ReadPreference(Parameters.ExtractToRegistryName);
        }

        [RelayCommand]
        async Task BeginExtractingAsync()
        {
            bool okToProceed = OkToBeginExtracting();

            if (!okToProceed)
            {
                return;
            }

            SetDefaultValues();

            List<string>? extractedFiles = await Extract();

            if (extractedFiles?.Count == 0)
            {
                UpdateInfoText = "No files were extracted";

                UpdateInfoTextColour = "#ff0000";

                StopTheClock();

                return;
            }

            await XMLCryExtraction.ConvertXmlFilesAsync(extractedFiles!);

            StopTheClock();

            UpdateInfoText = "Extraction completed";

            UpdateInfoTextColour = "#00ff00";
        }

        /// <summary>
        /// Check that we have all the variables we need to begin extracting
        /// </summary>
        /// <returns></returns>
        bool OkToBeginExtracting()
        {
            if (string.IsNullOrWhiteSpace(ExtractFromPath))
            {
                UpdateInfoText = "No p4k file selected";

                UpdateInfoTextColour = "#ff0000";

                return false;
            }

            if (string.IsNullOrWhiteSpace(ExtractToPath))
            {
                UpdateInfoText = "No output path selected";

                UpdateInfoTextColour = "#ff0000";

                return false;
            }

            if (ObservedFileTypes?.Count == 0)
            {
                UpdateInfoText = "Select file types for extraction";

                UpdateInfoTextColour = "#ff0000";

                return false;
            }

            if (AreWeExtracting)
            {
                AreWeExtracting = false;

                return false;
            }

            return true;
        }

        [RelayCommand]
        void CancelExtraction()
        {
            Parameters.CancelTokenSource.Cancel();

            StopTheClock();
        }

        [RelayCommand]
        void ResetExtractionList()
        {
            if (AreWeExtracting)
            {
                return;
            }

            ObservedFileTypes!.Clear();
        }

        [RelayCommand]
        void OnExtensionTypeClicked(object parameter)
        {
            if (AreWeExtracting)
            {
                return;
            }

            var extension = parameter as string;

            if (string.IsNullOrWhiteSpace(extension) ||
                !ObservedExtensions!.Contains(extension))
            {
                UpdateInfoText = "Unknown extension type";

                UpdateInfoTextColour = "#ff0000";

                return;
            }

            ClearUpdateInfoText();

            if (extension == Parameters.DefaultTypes[0])
            {
                foreach (var item in Parameters.DefaultXMLExtensions)
                {
                    if (!ObservedFileTypes!.Contains(item))
                    {
                        ObservedFileTypes.Add(item);
                    }
                }

                return;
            }

            if (extension == Parameters.DefaultTypes[1])
            {
                foreach (var item in Parameters.DefaultImageExtensions)
                {
                    if (!ObservedFileTypes!.Contains(item))
                    {
                        ObservedFileTypes.Add(item);
                    }
                }

                return;
            }

            if (extension == Parameters.DefaultTypes[2])
            {
                foreach (var item in ObservedExtensions)
                {
                    if (!ObservedFileTypes!.Contains(item) &&
                        !Parameters.DefaultTypes.Contains(item))
                    {
                        ObservedFileTypes.Add(item);
                    }
                }

                return;
            }

            if (ObservedFileTypes!.Contains(extension))
            {
                // this item is already in the list, so ignore it
                return;
            }

            ObservedFileTypes.Add(extension);
        }

        [RelayCommand]
        void OnFileTypeClicked(object parameter)
        {
            if (AreWeExtracting)
            {
                return;
            }

            var extension = parameter as string;

            if (string.IsNullOrWhiteSpace(extension) ||
                !ObservedFileTypes!.Contains(extension))
            {
                UpdateInfoText = "Unknown extension type";

                UpdateInfoTextColour = "#ff0000";

                return;
            }

            ClearUpdateInfoText();

            ObservedFileTypes.Remove(extension);
        }

        [RelayCommand]
        async Task OnSelectExtractionPathSelectionClickedAsync()
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            {
                DevicePlatform.WinUI, Parameters.WinDevicePlatform
            }
        });

            var options = new PickOptions
            {
                PickerTitle = Parameters.PickerP4K,

                FileTypes = customFileType,
            };

            var result = await FilePicker.Default.PickAsync(options);

            if (result?.FullPath is null)
            {
                return;
            }

            ExtractFromPath = result.FullPath;

            FileService.SavePreference(
                Parameters.ExtractFromRegistryName!,
                ExtractFromPath);
        }

        [RelayCommand]
        private async Task OnSelectExtractToPathSelectionClickedAsync()
        {
#pragma warning disable CA1416 // Validate platform compatibility
            var result = await FolderPicker.Default.PickAsync();
#pragma warning restore CA1416 // Validate platform compatibility

            if (result.IsSuccessful &&
                !string.IsNullOrWhiteSpace(result.Folder?.Path))
            {
                ExtractToPath = result.Folder.Path;

                FileService.SavePreference(
                    Parameters.ExtractToRegistryName!,
                    ExtractToPath);
            }
        }

        private void StopTheClock()
        {
            _stopwatch!.Stop();

            // stop updating the UI timer
            UiTimer?.Change(
                Timeout.Infinite,
                Timeout.Infinite);

            AreWeExtracting = false;
        }

        async Task<List<string>?> Extract()
        {
            List<string>? extensions = [];

            List<string> extractedFiles = await ExtractionService!.ExtractAsync(
                ExtractFromPath!,
                ExtractToPath!,
                ObservedFileTypes!,
                extensions);

            CheckForNewExtensions(extensions);

            return extractedFiles;
        }

        void CheckForNewExtensions(List<string> extensions)
        {
            if (extensions?.Count > 0)
            {
                foreach (var item in extensions)
                {
                    if (!ObservedExtensions!.Contains(item))
                    {
                        // new item extension found
                        AreNewExtensionsFound = true;
                    }

                    if (AreNewExtensionsFound)
                    {
                        break;
                    }
                }
            }
        }

        void OnTimerElapsed(object? state)
        {
            ExtractionTimer = _stopwatch!.Elapsed.ToString(@"hh\:mm\:ss");
        }

        void SetDefaultValues()
        {
            AreWeExtracting = true;

            ClearUpdateInfoText();

            FilesExtracted = 0;

            FilesConverted = 0;

            Parameters.CancelTokenSource = new CancellationTokenSource();

            _stopwatch = Stopwatch.StartNew();

            UiTimer = new(
                OnTimerElapsed,
                null,
                0,      // initial delay in milliseconds
                1000);  // set interval to 1 second
        }

        async Task GetExtensions(string[] separator)
        {
            ObservedExtensions!.Clear();

            using var stream = await FileSystem.OpenAppPackageFileAsync(Parameters.ExtensionsFile);

            using var reader = new StreamReader(stream);

            var contents = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(contents))
            {
                return;
            }

            var extensions = contents
                .Split(
                    separator,
                    StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (extensions?.Count == 0)
            {
                return;
            }

            foreach (var item in Parameters.DefaultTypes)
            {
                ObservedExtensions.Add(item);
            }

            foreach (var item in extensions!.OrderBy(o => o))
            {
                ObservedExtensions.Add(item);
            }
        }

        void ClearUpdateInfoText()
        {
            UpdateInfoText = "";

            AreNewExtensionsFound = false;
        }
    }
}

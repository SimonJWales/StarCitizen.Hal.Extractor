
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Input;
using Hal.Extractor.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Versioning;

namespace StarCitizen.Hal.Extractor.ViewModels
{
    [SupportedOSPlatform("windows")]
    public partial class MainPageViewModel : BaseViewModel
    {
        ExtractionService? ExtractionService { get; }

        XMLCryExtraction XMLCryExtraction { get; }

        AppState AppState { get; set; }

        public ObservableCollection<string>? ObservedFileTypes { get; } = [];

        public ObservableCollection<string>? ObservedExtensions { get; } = [];

        Stopwatch? _stopwatch;

        Timer? UiTimer { get; set; }

        public string AppVersion { get; set; }

        public MainPageViewModel(
            ExtractionService extractionService,
            XMLCryExtraction xMLCryExtraction,
            AppState appState)
        {
            ExtractionService = extractionService;

            XMLCryExtraction = xMLCryExtraction;

            AppState = appState;

            Title = Parameters.Title;

            AppVersion = GetVersion();

            ResetUpdateInfoText();

            Task.Run(async () =>
            {
                await GetExtensions(Parameters.AssetSeparator);
            });

            ExtractFromPath = FileService.ReadPreference(Parameters.ExtractFromPreference);

            ExtractToPath = FileService.ReadPreference(Parameters.ExtractToPreference);
        }

        [RelayCommand]
        async Task BeginExtractingAsync()
        {
            if (AppState.LogErrorState)
            {
                AppState!.SetErrorState(false);
            }

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

            ResetUpdateInfoText();

            if (Parameters.Defaults.TryGetValue(
                extension, 
                out List<string>? values))
            {
                // are we trying to add all extensions?
                if (extension.StartsWith("ALL"))
                {
                    ObservedFileTypes!.Clear();

                    foreach (var item in ObservedExtensions)
                    {
                        if (!ObservedFileTypes!.Contains(item) &&
                            !Parameters.Defaults.ContainsKey(item))
                        {
                            ObservedFileTypes.Add(item);
                        }
                    }
                    foreach (var item in ObservedExtensions!)
                    {
                        if (item.StartsWith("ALL"))
                        {
                            continue;
                        }

                        ObservedFileTypes.Add(item);
                    }

                    return;
                }

                foreach (var item in values)
                {
                    if (!ObservedFileTypes!.Contains(item))
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

            ResetUpdateInfoText();

            ObservedFileTypes.Remove(extension);
        }

        [SupportedOSPlatform("windows")]
        [RelayCommand]
        async Task OnExtractionPathSelectionClickedAsync()
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
                Parameters.ExtractFromPreference!,
                ExtractFromPath);
        }

        [SupportedOSPlatform("windows")]
        [RelayCommand]
        async Task OnOutputToPathSelectionClickedAsync()
        {
            var result = await FolderPicker.Default.PickAsync();

            if (result.IsSuccessful &&
                !string.IsNullOrWhiteSpace(result.Folder?.Path))
            {
                ExtractToPath = result.Folder.Path;

                FileService.SavePreference(
                    Parameters.ExtractToPreference!,
                    ExtractToPath);
            }
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

        void StopTheClock()
        {
            _stopwatch!.Stop();

            // stop updating the UI timer
            UiTimer?.Change(
                Timeout.Infinite,
                Timeout.Infinite);

            AreWeExtracting = false;
        }

        void OnTimerElapsed(object? state)
        {
            ExtractionTimer = _stopwatch!.Elapsed.ToString(@"hh\:mm\:ss");
        }

        void SetDefaultValues()
        {
            AreWeExtracting = true;

            ResetUpdateInfoText();

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

        /// <summary>
        /// Get the extensions list from the internal resource file
        /// </summary>
        /// <param name="separator"></param>
        /// <returns></returns>
        async Task GetExtensions(string[] separator)
        {
            ObservedExtensions!.Clear();

            // get the default extensions helper
            foreach (var item in Parameters.Defaults.Keys)
            {
                ObservedExtensions.Add(item);
            }

            // open file and read all extensions
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

            foreach (var item in extensions!.OrderBy(o => o))
            {
                ObservedExtensions.Add(item);
            }
        }

        /// <summary>
        /// Loop through the returned list of extensions and compare 
        /// against the observed extensions for any new ones
        /// </summary>
        /// <param name="extensions"></param>
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

        /// <summary>
        /// Clear the information text
        /// </summary>
        void ResetUpdateInfoText()
        {
            UpdateInfoText = "";

            AreNewExtensionsFound = false;
        }

        /// <summary>
        /// Get the version number from the assembly
        /// </summary>
        /// <returns></returns>
        string GetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            if (version is null ||
                string.IsNullOrWhiteSpace(version?.ToString()))
            {
                AppVersion = "v0.0.1a";
            }

            return $"v{version!.Major}.{version!.Minor}.{version!.Build}";
        }
    }
}

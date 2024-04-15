﻿
namespace Hal.Extractor.Services
{
    public class AppState
    {
        public Action? FileCountHasChanged;
        public int FileCount { get; set; }
        public void UpdateFileCount(int count)
        {
            FileCount = count;

            NotifyFileCountChanged();
        }
        void NotifyFileCountChanged()
        {
            FileCountHasChanged?.Invoke();
        }

        public Action? ConvertedCountHasChanged;
        public int ConvertedCount { get; set; }
        public void UpdateConvertedCount(int count)
        {
            ConvertedCount = count;

            NotifyConvertedCountChanged();
        }
        void NotifyConvertedCountChanged()
        {
            ConvertedCountHasChanged?.Invoke();
        }
    }
}
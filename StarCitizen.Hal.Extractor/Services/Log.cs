
using Hal.Extractor.Services;
using Microsoft.Extensions.Logging;

namespace StarCitizen.Hal.Extractor.Services
{
    public class Log(
        string path, 
        AppState? appstate) : ILogger
    {
        string _path = path;

        AppState? _appState = appstate;

        readonly object _lock = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return default;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        void ILogger.Log<TState>(
            LogLevel logLevel, 
            EventId eventId, 
            TState state, 
            Exception? exception, 
            Func<TState, Exception?, 
                string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            string message = $"{DateTime.UtcNow}: {formatter(state, exception)}";

            lock (_lock)
            {
                File.AppendAllText(
                    GetCurrentLogFilePath(), 
                    message + Environment.NewLine);
            }

            if (_appState!.LogErrorState is false)
            {
                _appState.SetErrorState(true);
            }
        }

        string GetCurrentLogFilePath()
        {
            var currentDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            return Path.Combine(
                _path, 
                $"halx_{currentDate}.log");
        }
    }
}

using System.Runtime.CompilerServices;
using ApiServer.Shared.Injectables;
using Microsoft.Extensions.Logging;

namespace ApiServer.Shared.Interfaces;

public interface IApiLogger : ITransientService
{
    void Log(LogLevel logLevel, string message,
    [CallerMemberName] string membername = "",
    [CallerFilePath] string filePath = "",
    [CallerLineNumber] int lineNumber = 0,
    params object[] args);  

    void LogTrace(string message,
        [CallerMemberName] string membername = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        params object[] args);
    void LogDebug(string message,
        [CallerMemberName] string membername = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        params object[] args);
    void LogInformation(string message,
        [CallerMemberName] string membername = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        params object[] args);
    void LogWarning(string message,
        [CallerMemberName] string membername = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        params object[] args);
    void LogError(string message,
        [CallerMemberName] string membername = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        params object[] args);
    void LogError(Exception exception, string message,
        [CallerMemberName] string membername = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        params object[] args);
}

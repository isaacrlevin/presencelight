using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PresenceLight.Core;

/// <summary>
/// This class is purposefully put without a namespace so that it overrides the existing ILogger extensions
/// to allow the additional context properties and log messaging to be written to the logs.
/// </summary>
public static class ILoggerExtensions
{

    /// <summary>
    /// Formats and writes a log message at the specified log level.
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="logLevel">Entry will be written on this level.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void Log(this ILogger logger, BaseConfig config,
                                      LogLevel logLevel,
                                      EventId eventId,
                                      Exception exception,
                                      string message,
                                      [CallerMemberName] string memberName = "",
                                      [CallerFilePath] string fileName = "",
                                      [CallerLineNumber] int lineNumber = 0,
                                      params object[] args)
    {
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

            logger.Log(logLevel, eventId, exception, message, args);
        }
    }



    /// <summary>
    /// Formats and writes a log message at the specified log level.
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="logLevel">Entry will be written on this level.</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void Log(this ILogger logger, BaseConfig config,
                                    LogLevel logLevel,
                                    EventId eventId,
                                    string message,
                                      [CallerMemberName] string memberName = "",
                                      [CallerFilePath] string fileName = "",
                                      [CallerLineNumber] int lineNumber = 0,
                                    params object[] args)
    {
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

            logger.Log(logLevel, eventId, message, args);
        }
    }

    /// <summary>
    /// Formats and writes a log message at the specified log level.
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="logLevel">Entry will be written on this level.</param> 
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void Log(this ILogger logger, BaseConfig config,
                                      LogLevel logLevel,
                                      Exception exception,
                                      string message,
                                      [CallerMemberName] string memberName = "",
                                      [CallerFilePath] string fileName = "",
                                      [CallerLineNumber] int lineNumber = 0, params object[] args)
    {
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";
            
            logger.Log(logLevel, exception, message, args);
        }
    }

    /// <summary>
    /// Formats and writes a log message at the specified log level.
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="logLevel">Entry will be written on this level.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void Log(this ILogger logger, BaseConfig config,
                                    LogLevel logLevel,
                                    string message,
                                    [CallerMemberName] string memberName = "",
                                    [CallerFilePath] string fileName = "",
                                    [CallerLineNumber] int lineNumber = 0, params object[] args)
    {
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

            logger.Log(logLevel, message, args);
        }

    }

    /// <summary>
    /// Formats and writes a critical log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogCritical(this ILogger logger, BaseConfig config,
                                            EventId eventId,
                                            Exception exception,
                                            string message,
                                            [CallerMemberName] string memberName = "",
                                            [CallerFilePath] string fileName = "",
                                            [CallerLineNumber] int lineNumber = 0,
                                            params object[] args)
    {
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

            logger.LogCritical(eventId, exception, message, args);
        }
    }

    /// <summary>
    /// Formats and writes a critical log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogCritical(this ILogger logger, BaseConfig config,
                                            EventId eventId,
                                            string message,
                                            [CallerMemberName] string memberName = "",
                                            [CallerFilePath] string fileName = "",
                                            [CallerLineNumber] int lineNumber = 0,
                                            params object[] args)
    {
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

            logger.LogCritical(eventId, message, args);
        }
    }

    /// <summary>
    /// Formats and writes a critical log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogCritical(this ILogger logger, BaseConfig config,
                                            Exception exception,
                                            string message,
                                            [CallerMemberName] string memberName = "",
                                            [CallerFilePath] string fileName = "",
                                            [CallerLineNumber] int lineNumber = 0,
                                            params object[] args)
    {
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

            logger.LogCritical(exception, message, args);
        }
    }


    /// <summary>
    /// Formats and writes a critical log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogCritical(this ILogger logger, BaseConfig config,
                                            string message,
                                            [CallerMemberName] string memberName = "",
                                            [CallerFilePath] string fileName = "",
                                            [CallerLineNumber] int lineNumber = 0,
                                            params object[] args)
    {
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

            logger.LogCritical(message, args);
        }
    }

    /// <summary>
    /// Formats and writes a debug log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogDebug(this ILogger logger, BaseConfig config,
                                         EventId eventId,
                                         Exception exception,
                                         string message,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string fileName = "",
                                         [CallerLineNumber] int lineNumber = 0,
                                         params object[] args)
    {
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

            logger.LogDebug(eventId, exception, message, args);
        }
    }

    /// <summary>
    /// Formats and writes a debug log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogDebug(this ILogger logger, BaseConfig config,
                                         EventId eventId,
                                         string message,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string fileName = "",
                                         [CallerLineNumber] int lineNumber = 0,
                                         params object[] args)
    {
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

            logger.LogDebug(eventId, message, args);
        }
    }
    ///<summary>
    /// Formats and writes a debug log message
    /// Enhanced for PresenceLight with extended Context Logging
    /// </summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogDebug(this ILogger logger, BaseConfig config,
                                     Exception exception,
                                     string message,
                                     [CallerMemberName] string memberName = "",
                                     [CallerFilePath] string fileName = "",
                                     [CallerLineNumber] int lineNumber = 0,
                                     params object[] args)
    {
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

            logger.LogDebug(exception, message, args);
        }
    }

    /// <summary>
    /// Formats and writes an error log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogDebug(this ILogger logger, BaseConfig config,
                                         string message,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string fileName = "",
                                         [CallerLineNumber] int lineNumber = 0,
                                         params object[] args)
    {
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

            logger.LogDebug(message, args);
        }
    }

    /// <summary>
    /// Formats and writes an error  log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogError(this ILogger logger, BaseConfig config,
                                         string message,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string fileName = "",
                                         [CallerLineNumber] int lineNumber = 0,
                                         params object[] args)
    {
        if (config.LogError)
        {
            using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
            using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
            using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
            {
                message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

                logger.LogError(message, args);
            }
        }
    }
    //
    // Summary:
    //     Formats and writes an error log message.
    //     Enhanced for PresenceLight with extended Context Logging
    //
    // Parameters:
    //   logger:
    //     The Microsoft.Extensions.Logging.ILogger to write to.
    //
    //   exception:
    //     The exception to log.
    //
    //   message:
    //     Format string of the log message in message template format. Example: "User {User}
    //     logged in from {Address}"
    //
    //   args:
    //     An object array that contains zero or more objects to format.

    /// <summary>
    /// Formats and writes an error  log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogError(this ILogger logger, BaseConfig config,
                                     Exception exception,
                                     string message,
                                     [CallerMemberName] string memberName = "",
                                     [CallerFilePath] string fileName = "",
                                     [CallerLineNumber] int lineNumber = 0,
                                     params object[] args)
    {
        if (config.LogError)
        {
            using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
            using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
            using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
            {
                message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

                logger.LogError(exception, message, args);
            }
        }
    }

    /// <summary>
    /// Formats and writes an error  log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogError(this ILogger logger, BaseConfig config,
                                         EventId eventId,
                                         string message,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string fileName = "",
                                         [CallerLineNumber] int lineNumber = 0,
                                         params object[] args)
    {
        if (config.LogError)
        {
            using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
            using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
            using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
            {
                message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

                logger.LogError(eventId, message, args);
            }
        }
    }

    /// <summary>
    /// Formats and writes an error  log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogError(this ILogger logger, BaseConfig config,
                                         EventId eventId,
                                         Exception exception,
                                         string message,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string fileName = "",
                                         [CallerLineNumber] int lineNumber = 0,
                                         params object[] args)
    {
        if (config.LogError)
        {
            using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
            using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
            using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
            {
                message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

                logger.LogError(eventId, exception, message, args);
            }
        }
    }

    /// <summary>
    /// Formats and writes an information log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogInformation(this ILogger logger, BaseConfig config,
                                               EventId eventId,
                                               Exception exception,
                                               string message,
                                               [CallerMemberName] string memberName = "",
                                               [CallerFilePath] string fileName = "",
                                               [CallerLineNumber] int lineNumber = 0,
                                               params object[] args)
    {
        if (config.LogInfo)
        {
            using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
            using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
            using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
            {
                message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

                logger.LogInformation(eventId, exception, message, args);
            }
        }
    }

    /// <summary>
    /// Formats and writes an information log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogInformation(this ILogger logger, BaseConfig config,
                                               EventId eventId,
                                               string message,
                                               [CallerMemberName] string memberName = "",
                                               [CallerFilePath] string fileName = "",
                                               [CallerLineNumber] int lineNumber = 0,
                                               params object[] args)
    {
        if (config.LogInfo)
        {
            using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
            using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
            using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
            {
                message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

                logger.LogInformation(eventId, message, args);
            }
        }
    }

    /// <summary>
    /// Formats and writes an information log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogInformation(this ILogger logger, BaseConfig config,
                                               Exception exception,
                                               string message,
                                               [CallerMemberName] string memberName = "",
                                               [CallerFilePath] string fileName = "",
                                               [CallerLineNumber] int lineNumber = 0,
                                               params object[] args)
    {
        if (config.LogInfo)
        {
            using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
            using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
            using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
            {
                message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

                logger.LogInformation(exception, message, args);
            }
        }
    }

    /// <summary>
    /// Formats and writes an information log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogInformation(this ILogger logger, BaseConfig config,
                                               string message,
                                               [CallerMemberName] string memberName = "",
                                               [CallerFilePath] string fileName = "",
                                               [CallerLineNumber] int lineNumber = 0,
                                               params object[] args)
    {
        if (config.LogInfo)
        {
            using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
            using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
            using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
            {
                message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

                logger.LogInformation(message, args);
            }
        }
    }


    /// <summary>
    /// Formats and writes a trace log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogTrace(this ILogger logger, BaseConfig config,
                                         EventId eventId,
                                         Exception exception,
                                         string message,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string fileName = "",
                                         [CallerLineNumber] int lineNumber = 0,
                                         params object[] args)
    {
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

            logger.LogTrace(eventId, exception, message, args);
        }
    }

    /// <summary>
    /// Formats and writes a trace log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogTrace(this ILogger logger, BaseConfig config,
                                         EventId eventId,
                                         string message,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string fileName = "",
                                         [CallerLineNumber] int lineNumber = 0,
                                         params object[] args)
    {
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

            logger.LogTrace(eventId, message, args);
        }
    }

    /// <summary>
    /// Formats and writes a trace log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogTrace(this ILogger logger, BaseConfig config,
                                         Exception exception,
                                         string message,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string fileName = "",
                                         [CallerLineNumber] int lineNumber = 0,
                                         params object[] args)
    {
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

            logger.LogTrace(exception, message, args);
        }
    }

    /// <summary>
    /// Formats and writes a trace log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogTrace(this ILogger logger, BaseConfig config,
                                         string message,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string fileName = "",
                                         [CallerLineNumber] int lineNumber = 0,
                                         params object[] args)
    {
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

            logger.LogTrace(message, args);
        }
    }


    /// <summary>
    /// Formats and writes a warning log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogWarning(this ILogger logger, BaseConfig config,
                                       EventId eventId,
                                       Exception exception,
                                       string message,
                                       [CallerMemberName] string memberName = "",
                                       [CallerFilePath] string fileName = "",
                                       [CallerLineNumber] int lineNumber = 0,
                                       params object[] args)
    {
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

            logger.LogWarning(eventId, exception, message, args);
        }
    }

    /// <summary>
    /// Formats and writes a warning log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="eventId">The event id associated with the log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogWarning(this ILogger logger, BaseConfig config,
                                       EventId eventId,
                                       string message,
                                       [CallerMemberName] string memberName = "",
                                       [CallerFilePath] string fileName = "",
                                       [CallerLineNumber] int lineNumber = 0,
                                       params object[] args)
    {
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

            logger.LogWarning(eventId, message, args);
        }
    }
    /// <summary>
    /// Formats and writes a warning log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogWarning(this ILogger logger, BaseConfig config,
                                       Exception exception,
                                       string message,
                                       [CallerMemberName] string memberName = "",
                                       [CallerFilePath] string fileName = "",
                                       [CallerLineNumber] int lineNumber = 0,
                                       params object[] args)
    {
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

            logger.LogWarning(exception, message, args);
        }
    }

    /// <summary>
    /// Formats and writes a warning log message
    /// Enhanced for PresenceLight with extended Context Logging</summary>
    /// <param name="logger">The Microsoft.Extensions.Logging.ILogger to write to.</param>
    /// <param name="config">Ref to Config to determine LogLevel</param>
    /// <param name="message">Format string of the log message.</param>
    /// <param name="memberName">Membername occurring Note:  Injected!</param>
    /// <param name="fileName">File name where occurring Note:  Injected!</param>
    /// <param name="lineNumber">LineNumber  where occurring Note:  Injected!</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    public static void LogWarning(this ILogger logger, BaseConfig config,
                                       string message,
                                       [CallerMemberName] string memberName = "",
                                       [CallerFilePath] string fileName = "",
                                       [CallerLineNumber] int lineNumber = 0,
                                       params object[] args)
    {        
        using (Serilog.Context.LogContext.PushProperty("MemberName", memberName))
        using (Serilog.Context.LogContext.PushProperty("FilePath", fileName))
        using (Serilog.Context.LogContext.PushProperty("LineNumber", lineNumber))
        {
            message = $"{message} - {fileName.Split("\\").LastOrDefault().Replace(".cs", "")}:{memberName} Line: {lineNumber}";

            logger.LogWarning(message, args);
        }
    }
}


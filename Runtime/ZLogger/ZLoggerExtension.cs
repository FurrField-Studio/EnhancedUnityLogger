#if ZLOGGER

using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
using Microsoft.Extensions.Logging;
using ZLogger;

public static class ZLoggerExtension
{

    /// <summary>
    /// Standard logging function, priority will default to info level
    /// </summary>
    /// <param name="logChannel"></param>
    /// <param name="message"></param>
    public static void ULog(this ILogger logger, LoggerChannel logChannel, string message)
    {
        FinalLog(logger, logChannel, Priority.Info, message);
    }

    /// <summary>
    /// Standard logging function with specified priority
    /// </summary>
    /// <param name="logChannel"></param>
    /// <param name="priority"></param>
    /// <param name="message"></param>
    public static void ULog(this ILogger logger, LoggerChannel logChannel, Priority priority, string message)
    {
        FinalLog(logger, logChannel, priority, message);
    }

    /// <summary>
    /// Log with format args, priority will default to info level
    /// </summary>
    /// <param name="logChannel"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void ULog(this ILogger logger, LoggerChannel logChannel, string message, params object[] args)
    {
        FinalLog(logger, logChannel, Priority.Info, string.Format(message, args));
    }

    /// <summary>
    /// Log with format args and specified priority
    /// </summary>
    /// <param name="logChannel"></param>
    /// <param name="priority"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void ULog(this ILogger logger, LoggerChannel logChannel, Priority priority, string message, params object[] args)
    {
        FinalLog(logger, logChannel, priority,  string.Format(message, args));
    }

    /// <summary>
    /// Assert that the passed in condition is true, otherwise log a fatal error
    /// </summary>
    /// <param name="condition">The condition to test</param>
    /// <param name="message">A user provided message that will be logged</param>
    public static void Assert(this ILogger logger, bool condition, string message)
    {
        if (!condition)
        {
            FinalLog(logger, LoggerChannel.Assert, Priority.FatalError, ZString.Format("Assert Failed: {0}", message));
        }
    }

    /// <summary>
    /// This function controls where the final string goes
    /// </summary>
    /// <param name="logChannel"></param>
    /// <param name="priority"></param>
    /// <param name="message"></param>
    private static void FinalLog(ILogger logger, LoggerChannel logChannel, Priority priority, string message)
    {
        if (Logger.IsChannelActive(logChannel))
        {
#if UNITY_EDITOR && UNITY_DIALOGS
            // Fatal errors will create a pop up when in the editor
            if (priority == Priority.FatalError)
			{
			    bool ignore = EditorUtility.DisplayDialog("Fatal error", finalMessage, "Ignore", "Break");
			    if (!ignore)
			    {
			        Debug.Break();
			    }
            }
#endif

            bool shouldColour = (priority != Priority.FatalError);

            // Call the correct unity logging function depending on the type of error 
            switch (priority)
            {
                case Priority.FatalError:
                case Priority.Error:
                    if (shouldColour)
                    {
                        logger.ZLogError(COLORED_MESSAGE, GetColor(logChannel), logChannel, Logger.PriorityToColour[priority], message);
                    }
                    else
                    {
                        logger.ZLogError(RAW_MESSAGE, logChannel, message);
                    }
                    break;

                case Priority.Warning:
                    if (shouldColour)
                    {
                        logger.ZLogWarning(COLORED_MESSAGE, GetColor(logChannel), logChannel, Logger.PriorityToColour[priority], message);
                    }
                    else
                    {
                        logger.ZLogWarning(RAW_MESSAGE, logChannel, message);
                    }
                    break;

                case Priority.Info:
                    if (shouldColour)
                    {
                        logger.ZLogInformation(COLORED_MESSAGE, GetColor(logChannel), logChannel, Logger.PriorityToColour[priority], message);
                    }
                    else
                    {
                        logger.ZLogInformation(RAW_MESSAGE, logChannel, message);
                    }
                    break;
            }
        }
    }

    private static string GetColor(LoggerChannel logChannel)
    {
        string channelColour;
        if (!Logger.channelToColour.TryGetValue(logChannel, out channelColour))
        {
#if UNITY_PRO_LICENSE || UNITY_2019_4_OR_NEWER
            channelColour = "white";
#else
            channelColour = "black";
#endif
        }

        return channelColour;
    }
    
    private const string COLORED_MESSAGE = "<b><color={0}>[{1}] </color></b> <color={2}>{3}</color>";
    private const string RAW_MESSAGE = "[{0}] {1}";
}

#endif

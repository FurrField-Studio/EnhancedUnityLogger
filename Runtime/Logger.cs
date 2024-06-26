﻿#define UNITY_DIALOGS // Comment out to disable dialogs for fatal errors
using System;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR && UNITY_DIALOGS
using UnityEditor;
#endif

/// <summary>
/// The priority of the log
/// </summary>
public enum Priority
{
    // Default, simple output about game
    Info,
    // Warnings that things might not be as expected
    Warning,
    // Things have already failed, alert the dev
    Error,
    // Things will not recover, bring up pop up dialog
    FatalError,
}

public partial class Logger
{
    #region SingletonSetup

    private static Logger m_InternalInstance;
    private static Logger Instance
    {
        get
        {
            return m_InternalInstance ?? (m_InternalInstance = new Logger());
        }

    }

    private Logger()
    {
        m_InternalInstance = this;
        m_InternalInstance.m_Channels = new Dictionary<LoggerChannel, bool>();
        AddAllChannels();
    }

    #endregion

    #region Members
    
    private Dictionary<LoggerChannel, bool> m_Channels;

    public delegate void OnLogFunc(LoggerChannel channel, Priority priority, string message);
    public static event OnLogFunc OnLog;

    #endregion

    #region ChannelControl

    public static void ResetChannels()
    {
        AddAllChannels();
    }

    public static void AddChannel(LoggerChannel channelToAdd)
    {
        Instance.m_Channels.Add(channelToAdd, true);
    }

    public static void RemoveChannel(LoggerChannel channelToRemove)
    {
        Instance.m_Channels.Remove(channelToRemove);
    }

    public static void ToggleChannel(LoggerChannel channelToToggle)
    {
        Instance.m_Channels[channelToToggle] = !Instance.m_Channels[channelToToggle];
    }

    public static bool IsChannelActive(LoggerChannel channelToCheck)
    {
        return Instance.m_Channels.ContainsKey(channelToCheck) && Instance.m_Channels[channelToCheck];
    }

    public static void SetChannels(Dictionary<LoggerChannel, bool> channelsToSet)
    {
        Instance.m_Channels = channelsToSet;
    }

    private static void AddAllChannels()
    {
        m_InternalInstance.m_Channels?.Clear();

        foreach (uint channel in Enum.GetValues(typeof(LoggerChannel)))
        {
            m_InternalInstance.m_Channels.Add((LoggerChannel) channel, true);
        }
    }

    #endregion
    
    #region Logging

    /// <summary>
    /// Standard logging function, priority will default to info level
    /// </summary>
    /// <param name="logChannel"></param>
    /// <param name="message"></param>
    public static void Log(LoggerChannel logChannel, object message)
    {
        FinalLog(logChannel, Priority.Info, message);
    }

    /// <summary>
    /// Standard logging function with specified priority
    /// </summary>
    /// <param name="logChannel"></param>
    /// <param name="priority"></param>
    /// <param name="message"></param>
    public static void Log(LoggerChannel logChannel, Priority priority, object message)
    {
        FinalLog(logChannel, priority, message);
    }

    /// <summary>
    /// Log with format args, priority will default to info level
    /// </summary>
    /// <param name="logChannel"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Log(LoggerChannel logChannel, string message, params object[] args)
    {
        FinalLog(logChannel, Priority.Info, string.Format(message, args));
    }

    /// <summary>
    /// Log with format args and specified priority
    /// </summary>
    /// <param name="logChannel"></param>
    /// <param name="priority"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    public static void Log(LoggerChannel logChannel, Priority priority, string message, params object[] args)
    {
        FinalLog(logChannel, priority, string.Format(message, args));
    }

    /// <summary>
    /// Assert that the passed in condition is true, otherwise log a fatal error
    /// </summary>
    /// <param name="condition">The condition to test</param>
    /// <param name="message">A user provided message that will be logged</param>
    public static void Assert(bool condition, object message)
    {
        if (!condition)
        {
            FinalLog(LoggerChannel.Assert, Priority.FatalError, string.Format("Assert Failed: {0}", message));
        }
    }

    /// <summary>
    /// This function controls where the final string goes
    /// </summary>
    /// <param name="logChannel"></param>
    /// <param name="priority"></param>
    /// <param name="message"></param>
    private static void FinalLog(LoggerChannel logChannel, Priority priority, object message)
    {
        if (IsChannelActive(logChannel))
        {
            // Dialog boxes can't support rich text mark up, do we won't colour the final string 
			string finalMessage = ContructFinalString(logChannel, priority, message.ToString(), (priority != Priority.FatalError));

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
            // Call the correct unity logging function depending on the type of error 
            switch (priority)
            {
                case Priority.FatalError:
                case Priority.Error:
                    Debug.LogError(finalMessage);
                    break;

                case Priority.Warning:
                    Debug.LogWarning(finalMessage);
                    break;

                case Priority.Info:
                    Debug.Log(finalMessage);
                    break;
            }

            if(OnLog != null)
            {
                OnLog.Invoke(logChannel, priority, finalMessage);
            }
        }
    }

    /// <summary>
    /// Creates the final string with colouration based on channel and priority 
    /// </summary>
    /// <param name="logChannel"></param>
    /// <param name="priority"></param>
    /// <param name="message"></param>
    /// <param name="shouldColour"></param>
    /// <returns></returns>
    private static string ContructFinalString(LoggerChannel logChannel, Priority priority, string message, bool shouldColour)
    {
        if(shouldColour)
        {
            if(!channelToColour.TryGetValue(logChannel, out string channelColour))
            {
#if UNITY_PRO_LICENSE || UNITY_2019_4_OR_NEWER
                channelColour = "white";
#else
                channelColour = "black";
#endif
            }
            
            return string.Format(COLORED_MESSAGE, channelColour, logChannel, PriorityToColour[priority], message);
        }

        return string.Format(RAW_MESSAGE, logChannel, message);
    }

    #endregion

    private const string COLORED_MESSAGE = "<b><color={0}>[{1}] </color></b> <color={2}>{3}</color>";
    private const string RAW_MESSAGE = "[{0}] {1}";
    
    public static readonly Dictionary<Priority, string> PriorityToColour = new Dictionary<Priority, string>
    {
#if UNITY_PRO_LICENSE || UNITY_2019_4_OR_NEWER
        { Priority.Info,        "white" },
#else
        { Priority.Info,        "black" },
#endif
        { Priority.Warning,     "orange" },
        { Priority.Error,       "red" },
        { Priority.FatalError,  "red" },
    };
}

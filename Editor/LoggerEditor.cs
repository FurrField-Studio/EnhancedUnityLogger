using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class LoggerEditor : EditorWindow
{
    private static LoggerEditor Instance;
    
    [MenuItem("Tools/Enhanced Logger Window")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow(typeof(LoggerEditor), false, "Enhanced Logger Window", true);
        Instance = (LoggerEditor) window;
        Instance.GetAllChannels();
    }

    private void GetAllChannels()
    {
        if (m_Channels == null) m_Channels = new List<Channel>();
        m_Channels.Clear();

        Color color;
        foreach (uint channel in Enum.GetValues(typeof(LoggerChannel)))
        {
            if (Logger.channelToColour.ContainsKey((LoggerChannel)channel))
            {
                ColorUtility.TryParseHtmlString(Logger.channelToColour[(LoggerChannel)channel], out color);
            }
            else
            {
                color = Color.black;
            }
            m_Channels.Add(new Channel(channel, Enum.GetName(typeof(LoggerChannel), channel), color));
        }
    }

    [SerializeField]
    private List<Channel> m_Channels;

    private void OnGUI()
    {
        if (m_Channels == null) GetAllChannels();
        
        EditorGUI.BeginChangeCheck();
 
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear all"))
        {
            ActiveAll(false);
        }
        if (GUILayout.Button("Select all"))
        {
            ActiveAll(true);
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Label("Click to toggle logging channels", EditorStyles.boldLabel);
        
        foreach (Channel channel in m_Channels.ToArray())
        {
            EditorGUILayout.BeginHorizontal();
            
            channel.Enabled = GUILayout.Toggle(channel.Enabled, "", GUILayout.ExpandWidth(false));
            channel.Name = GUILayout.TextField(channel.Name, GUILayout.ExpandWidth(true));
            channel.Color = EditorGUILayout.ColorField(channel.Color, GUILayout.ExpandWidth(false));

            if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
            {
                m_Channels.Remove(channel);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        GUILayout.Label("");
        if (GUILayout.Button("Add channel"))
        {
            m_Channels.Add(new Channel((uint) m_Channels.Count, "NewChannel", Color.black));
        }
        
        GUILayout.Label("");
        if (GUILayout.Button("Generate scripts"))
        {
            LoggerGenerator.GenerateChannelsScripts(m_Channels, GetLoggerPath());
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }

        // If the game is playing then update it live when changes are made
        if (EditorApplication.isPlaying && EditorGUI.EndChangeCheck())
        {
            Logger.SetChannels(ChannelsToRuntimeChannelList(m_Channels));
        }
    }
    
    // When the game starts update the logger instance with the users selections
    private void OnEnable()
    {
        if (m_Channels == null) GetAllChannels();
        
        Logger.SetChannels(ChannelsToRuntimeChannelList(m_Channels));
    }

    private void ActiveAll(bool value)
    {
        foreach (Channel channel in m_Channels)
        {
            channel.Enabled = value;
        }
    }

    #region Data

    private class Channel
    {
        public uint Id;
        public string Name;
        public bool Enabled;
        public Color Color;

        public Channel(uint id, string name, Color color)
        {
            Id = id;
            Name = name;
            Enabled = true;
            Color = color;
        }
    }

    #endregion

    #region Helpers

    private string GetLoggerPath()
    {
        MonoScript ms = MonoScript.FromScriptableObject( this );
        string scriptFilePath = AssetDatabase.GetAssetPath( ms );
 
        FileInfo fi = new FileInfo( scriptFilePath);
        string scriptFolder = fi.Directory.ToString();
        scriptFolder = scriptFolder.Replace('\\', '/');
        scriptFolder = scriptFolder.Remove(scriptFolder.Length - 7, 7);
        
        return scriptFolder;
    }

    private Dictionary<LoggerChannel, bool> ChannelsToRuntimeChannelList(List<Channel> channels)
    {
        Dictionary<LoggerChannel, bool> runtimeChannelList = new Dictionary<LoggerChannel, bool>();

        foreach (var channel in channels)
        {
            runtimeChannelList.Add((LoggerChannel) channel.Id, channel.Enabled);
        }

        return runtimeChannelList;
    }

    #endregion

    private static class LoggerGenerator
    {
        private const string CHANNEL_FILE_TEMPLATE = "LoggerChannelTemplate.txt";
        private const string DATA_FILE_TEMPLATE = "LoggerDataTemplate.txt";
        
        private const string CHANNEL_FILE = "LoggerChannel.cs";
        private const string DATA_FILE = "LoggerData.cs";

        private const string DATA_REPLACE = "%DATA%";

        public static void GenerateChannelsScripts(List<Channel> data, string path)
        {
            StringBuilder channelBuilder = new StringBuilder();
            StringBuilder coloringBuilder = new StringBuilder();
            
            data.Sort((a, b) => a.Id.CompareTo(b.Id));

            foreach (var channel in data)
            {
                //channel data
                channelBuilder.Append("\t");
                channelBuilder.Append(channel.Name);
                channelBuilder.Append('=');
                channelBuilder.Append(channel.Id);
                channelBuilder.Append(",\n");

                //coloring data
                coloringBuilder.Append("\t\t{");
                coloringBuilder.Append("LoggerChannel.");
                coloringBuilder.Append(channel.Name);
                coloringBuilder.Append(',');
                coloringBuilder.Append("\"#");
                coloringBuilder.Append(ColorUtility.ToHtmlStringRGB(channel.Color));
                coloringBuilder.Append("\"},\n");
            }

            string channelTemplateData = File.ReadAllText(path + "\\Editor\\" + CHANNEL_FILE_TEMPLATE);
            string coloringTemplateData = File.ReadAllText(path + "\\Editor\\" + DATA_FILE_TEMPLATE);

            channelTemplateData = channelTemplateData.Replace(DATA_REPLACE, channelBuilder.ToString());
            coloringTemplateData = coloringTemplateData.Replace(DATA_REPLACE, coloringBuilder.ToString());

            File.WriteAllText(path + "\\Runtime\\" + CHANNEL_FILE, channelTemplateData);
            File.WriteAllText(path + "\\Runtime\\" + DATA_FILE, coloringTemplateData);
        }
    }
}

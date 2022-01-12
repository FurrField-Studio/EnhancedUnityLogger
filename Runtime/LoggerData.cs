using System.Collections.Generic;

public partial class Logger
{
    public static readonly Dictionary<LoggerChannel, string> channelToColour = new Dictionary<LoggerChannel, string>
    {
		{LoggerChannel.AI,"#0000FF"},
		{LoggerChannel.Rendering,"#008000"},
		{LoggerChannel.Physics,"#FFFF00"},
		{LoggerChannel.UI,"#800080"},
		{LoggerChannel.Audio,"#008080"},
		{LoggerChannel.Loading,"#808000"},
		{LoggerChannel.Localisation,"#A52A2A"},
		{LoggerChannel.Platform,"#FF0000"},
		{LoggerChannel.Assert,"#FF0000"},
		{LoggerChannel.Build,"#000080"},
		{LoggerChannel.Analytics,"#800000"},
		{LoggerChannel.Animation,"#000000"},
		{LoggerChannel.Player,"#00F6FF"},

    };
}
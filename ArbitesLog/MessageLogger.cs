using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using System.Text.Json;
using System.IO;
namespace ArbitesLog
{
	public class MessageLogger
	{

		public static void LogMessage(SocketMessage msg)
		{
			LoggedMessage message = new LoggedMessage
			{
				MessageID = msg.Id,
				AuthorID = msg.Author.Id,
				ChannelID = msg.Channel.Id,
				MessageContent = msg.Content,
				Timestamp = msg.Timestamp
			};

			JsonSerializerOptions options = new JsonSerializerOptions
			{
				WriteIndented = true
			};

			string jsonout = JsonSerializer.Serialize(message, options);
			File.WriteAllText("\\" + ((SocketTextChannel)msg.Channel).Guild.Id.ToString() + "\\" + msg.Id.ToString() + ".json", jsonout);
		}
	}
}

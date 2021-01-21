using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Text.Json;
using System.IO;
namespace ArbitesLog
{
	public class MessageLogger
	{

		public static async Task LogMessage(SocketMessage msg)
		{
			LoggedMessage message = new LoggedMessage
			{
				MessageID = msg.Id,
				AuthorID = msg.Author.Id,
				ChannelID = msg.Channel.Id,
				Timestamp = msg.Timestamp
			};
			message.MessageContent.Add(msg.Content);

			JsonSerializerOptions options = new JsonSerializerOptions
			{
				WriteIndented = true
			};
			
			string jsonout = JsonSerializer.Serialize(message, options);
			if(!Directory.Exists("Logs/"+((SocketTextChannel)msg.Channel).Guild.Id.ToString() + "/"))
			{
				Directory.CreateDirectory("Logs/"+((SocketTextChannel)msg.Channel).Guild.Id.ToString() + "/");
			}
			await File.WriteAllTextAsync("Logs/"+((SocketTextChannel)msg.Channel).Guild.Id.ToString() + "/" + msg.Id.ToString() + ".json", jsonout);
		}
		public static async Task<LoggedMessage> GetLog(ulong msgID, ulong guildID)
		{
			string jsonIn = await File.ReadAllTextAsync("Logs/"+guildID.ToString() + "/" + msgID + ".json");
			return JsonSerializer.Deserialize<LoggedMessage>(jsonIn);
		}

		public static async Task UpdateLog(ulong msgID, ulong guildID, LoggedMessage updatedMessage)
		{
			string jsonOut = JsonSerializer.Serialize(updatedMessage);
			await File.WriteAllTextAsync("Logs/" + guildID.ToString() + "/" + msgID + ".json", jsonOut);
		}
	}
}

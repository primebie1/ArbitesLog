using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace ArbitesLog
{
	public class LoggerModule:ModuleBase<SocketCommandContext>
	{
		[Command("Access")]
		[Summary("Accesses Message Details From Log")]
		public async Task AccessAsync(uint iD)
		{
			string jsonIn = File.ReadAllText("\\" + Context.Guild.Id.ToString() + "\\" + iD + ".json");
			LoggedMessage message = JsonSerializer.Deserialize<LoggedMessage>(jsonIn);
			var embed = new EmbedBuilder();
			var em = embed.AddField("Message " + message.MessageID.ToString(), message.MessageContent)
				.WithAuthor(Context.Guild.GetUser(message.AuthorID))
				.WithColor(Color.Blue)
				.WithFooter(footer => footer.Text = "ArbitiesLog")
				.WithTimestamp(message.Timestamp)
				.Build();
			await ReplyAsync(embed: em);
		}


	}
}

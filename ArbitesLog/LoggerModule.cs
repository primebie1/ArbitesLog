using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace ArbitesLog
{
	[RequireUserPermission(GuildPermission.Administrator)]
	public class LoggerModule:ModuleBase<SocketCommandContext>
	{
		[Command("Access")]
		[Summary("Accesses Message Details From Log")]
		public async Task AccessAsync(ulong iD)
		{
			LoggedMessage message = await MessageLogger.GetLog(iD, Context.Guild.Id);

			var embed = new EmbedBuilder();
			var em = embed.AddField("Message " + message.MessageID.ToString(), message.MessageContent)
				.WithAuthor(Context.Guild.GetUser(message.AuthorID))
				.WithColor(Color.Blue)
				.WithFooter(footer => footer.Text = "ArbitiesLog")
				.WithTimestamp(message.Timestamp)
				.Build();

			await ReplyAsync(embed: em);
		}

		[Command("SetLog")]
		[Summary("Sets A Channel For Logging")]
		public async Task SetLogAsync(SocketChannel channel)
		{
			ulong chanID = channel.Id;
			
			if (GuildManager.CheckGuildData(Context.Guild.Id))
			{
				GuildData data = await GuildManager.GetGuildData(Context.Guild.Id);
				data.LogChannel = chanID;
				await GuildManager.SetGuildData(data);
			}
			else
			{
				GuildData data = new GuildData
				{
					GuildID = Context.Guild.Id,
					LogChannel = chanID
				};
				await GuildManager.SetGuildData(data);
			}
		}


	}
}

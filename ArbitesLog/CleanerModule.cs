using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.IO;
using System.Text.Json;

namespace ArbitesLog
{
	public class CleanerModule:ModuleBase<SocketCommandContext>
	{
		[Command("NextCleanup")]
		[Summary("Gives the time till the next automatic cleanup")]
		public async Task NextCleanAsync()
		{
			Config config = JsonSerializer.Deserialize<Config>(File.ReadAllText("Config/config.json"));
			TimeSpan span = config.CleanupTime.TimeOfDay - DateTime.Now.TimeOfDay;
			var embed = new EmbedBuilder();
			var em = embed.AddField("Time to next Cleanup", span.ToString("c"))
				.WithColor(Color.DarkGreen)
				.WithFooter(footer => footer.Text = "ArbitiesLog")
				.WithTimestamp(DateTimeOffset.Now)
				.Build();
			await ReplyAsync(embed: em);
		}
		[RequireUserPermission(GuildPermission.Administrator)]
		[Command("RunCleanup")]
		[Summary("Runs log cleanup")]
		public async Task RunCleanAsync()
		{
			Config config = JsonSerializer.Deserialize<Config>(File.ReadAllText("Config/config.json"));
			await DataManager.RunCleanup(config.MessageTimeToDie);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;


namespace ArbitesLog
{
	class GenUtilModule : ModuleBase<SocketCommandContext>
	{
		[Command("BotInfo")]
		[Summary("Displays info on bot")]
		public async Task BotInfoAsync()
		{
			var vernum = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
			var app = await Context.Client.GetApplicationInfoAsync();
			await ReplyAsync($"```\nArbitesLog\n\n/========General Info========\\ \nVersion {vernum}\nOwned by {app.Owner}\nBuilt With Discord.NET version {DiscordConfig.Version}\nRunning on {RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture} On {RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture}\n\n/========Stats========\\ \nHeap Size: {GetHeapSize()}MiB\nGuilds Connected: {Context.Client.Guilds.Count}\nChannels: {Context.Client.Guilds.Sum(g => g.Channels.Count)}\nUsers: {Context.Client.Guilds.Sum(g => g.Users.Count)}\nUptime: {GetUptime()}\n```");
		}
		private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
		private static string GetUptime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;


namespace ArbitesLog
{
	public class CommandHandler
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;
		private readonly IServiceProvider _services;
		private readonly Config _config;
		public CommandHandler(DiscordSocketClient client, CommandService commands, Config config)
		{
			_client = client;
			_commands = commands;
			_config = config;
			_services = ConfigureServices();
		}

		private async Task HandleCommandAsync(SocketMessage arg)
		{
			if (arg.Author == _client.CurrentUser) return;
			await MessageLogger.LogMessage(arg);
			if (!(arg is SocketUserMessage msg)) return;

			if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot) return;

			int pos = 0;
			//check for prefix
			if (msg.HasCharPrefix(_config.Prefix, ref pos))
			{
				var context = new SocketCommandContext(_client, msg);

				var result = await _commands.ExecuteAsync(context, pos, _services);

				if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
					await msg.Channel.SendMessageAsync(result.ErrorReason);
			}
		}

		public async Task InitCommands()
		{
			await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

			_client.MessageReceived += HandleCommandAsync;
			_commands.CommandExecuted += Commands_CommandExecuted;
		}
		private Task Commands_CommandExecuted(Optional<CommandInfo> arg1, ICommandContext arg2, IResult arg3)
		{
			if (arg3.IsSuccess)
			{
				StringBuilder stringBuilder = new StringBuilder().Append("Command: ").Append(arg1.Value.Name).Append(" In Guild: ").Append(arg2.Guild.Name).Append(" Channel: ").Append(arg2.Channel.Name).Append(" Executed Successfuly!");
				LogMessage msg = new LogMessage(LogSeverity.Info, "CommandHandler", stringBuilder.ToString());
				return Program.Log(msg);
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder().Append("Command: ").Append(arg1.Value.Name).Append(" In Guild: ").Append(arg2.Guild.Name).Append(" Channel: ").Append(arg2.Channel.Name).Append(" Failed! Reason: ").Append(arg3.ErrorReason);
				LogMessage msg = new LogMessage(LogSeverity.Info, "CommandHandler", stringBuilder.ToString());
				return Program.Log(msg);

			}
		}

		private static IServiceProvider ConfigureServices()
		{
			var map = new ServiceCollection()
				.AddSingleton(new CommandService());

			return map.BuildServiceProvider();
		}

	}
}

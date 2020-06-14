using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Discord.Commands;
using Discord.API;
using Discord.Audio;
using Discord.Rest;
using Discord.Webhook;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;

namespace ArbitesLog
{
	class Program
	{
		public static void Main() => new Program().MainAsync().GetAwaiter().GetResult();

		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;
		private readonly IServiceProvider _services;
		private readonly string confFile = "config.json";
		private Config config = new Config();

		public async Task MainAsync()
		{
			await InitCommands();

			await _client.LoginAsync(TokenType.Bot, config.Token);
			await _client.StartAsync();

			// Block this task until the program is closed.
			await Task.Delay(-1);
		}

		private Program()
		{
			//setup check
			if (File.Exists(confFile))
			{
				ConfLoad();
			}
			else
			{
				ConfSetup();
			}

			//configure SocketClient
			_client = new DiscordSocketClient(new DiscordSocketConfig
			{
				LogLevel = LogSeverity.Info,
			});
			//Create and configure Command Service
			_commands = new CommandService(new CommandServiceConfig
			{
				LogLevel = LogSeverity.Info,

				CaseSensitiveCommands = false,
			});

			_client.Log += Log;
			_commands.Log += Log;
			_services = ConfigureServices();
		}

		//First Time Setup
		void ConfSetup()
		{
			//create config from user input
			Console.WriteLine("Welcome to ArbitiesLog! This first run configuration will setup ArbitiesLog to run on your system!");
			Console.WriteLine("Please input your Bot Token:");
			config.Token = Console.ReadLine();
			Console.WriteLine("Please Input your prefered prefix for commands:");
			config.Prefix = Console.ReadLine()[0];

			JsonSerializerOptions options = new JsonSerializerOptions
			{
				WriteIndented = true // write pretty json
			};

			//serialise and save settings for future launches
			string jsonstring = JsonSerializer.Serialize(config, options);
			File.WriteAllText(confFile, jsonstring);
		}

		void ConfLoad()
		{
			//load master config
			string jsonstring = File.ReadAllText(confFile);
			config = JsonSerializer.Deserialize<Config>(jsonstring);
		}

		private static IServiceProvider ConfigureServices()
		{
			var map = new ServiceCollection()
				.AddSingleton(new CommandService());

			return map.BuildServiceProvider();
		}

		private async Task InitCommands()
		{
			await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

			_client.MessageReceived += HandleCommandAsync;
		}

		private async Task HandleCommandAsync(SocketMessage arg)
		{
			MessageLogger.LogMessage(arg);
			if (!(arg is SocketUserMessage msg)) return;

			if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot) return;

			int pos = 0;
			//check for prefix
			if (msg.HasCharPrefix(config.Prefix, ref pos))
			{
				var context = new SocketCommandContext(_client, msg);

				var result = await _commands.ExecuteAsync(context, pos, _services);

				if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
					await msg.Channel.SendMessageAsync(result.ErrorReason);
			}
		}

		private static Task Log(LogMessage message)
		{
			switch (message.Severity)
			{
				case LogSeverity.Critical:
				case LogSeverity.Error:
					Console.ForegroundColor = ConsoleColor.Red;
					break;
				case LogSeverity.Warning:
					Console.ForegroundColor = ConsoleColor.Yellow;
					break;
				case LogSeverity.Info:
					Console.ForegroundColor = ConsoleColor.White;
					break;
				case LogSeverity.Verbose:
				case LogSeverity.Debug:
					Console.ForegroundColor = ConsoleColor.DarkGray;
					break;
			}
			Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
			Console.ResetColor();

			return Task.CompletedTask;
		}

	}
}

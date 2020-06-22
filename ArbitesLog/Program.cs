using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
using System.Timers;
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
		private readonly string confFile = "Config\\config.json";
		private Config config = new Config();
		private Timer cleanupTimer;

		public async Task MainAsync()
		{
			await InitCommands();
			InitLogger();
			InitCleaner();

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
			Directory.CreateDirectory("Config\\");
			Directory.CreateDirectory("Logs\\");
			//create config from user input
			Console.WriteLine("Welcome to ArbitiesLog! This first run configuration will setup ArbitiesLog to run on your system!");
			Console.WriteLine("Please input your Bot Token:");
			config.Token = Console.ReadLine();
			Console.WriteLine("Please Input your prefered prefix for commands:");
			config.Prefix = Console.ReadLine()[0];
			Console.WriteLine("Please Input the time for running cleanup (24 hr time)");
			string timeIn = Console.ReadLine();
			string[] timeSplit = timeIn.Split(' ');
			int.TryParse(timeSplit[0], out int hour);
			int.TryParse(timeSplit[1], out int min);
			config.CleanupTime = new DateTime(1, 1, 11, hour, min, 0);

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

		private void InitLogger()
		{
			 _client.MessageDeleted += HandleMessageDeleteAsync; 
		}
		private async Task InitCommands()
		{
			await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

			_client.MessageReceived += HandleCommandAsync;
			_commands.CommandExecuted += Commands_CommandExecuted;
		}

		private Task Commands_CommandExecuted(Optional<CommandInfo> arg1, ICommandContext arg2, IResult arg3)
		{
			throw new NotImplementedException();
		}

		private void InitCleaner()
		{
			cleanupTimer = new Timer(GetCleanerOffset())
			{
				AutoReset = false,
				Enabled = true,
			};
			cleanupTimer.Elapsed += TimerEnded;
			cleanupTimer.Start();
		}

		private double GetCleanerOffset()
		{
			return config.CleanupTime.TimeOfDay.TotalMilliseconds - DateTime.Now.TimeOfDay.TotalMilliseconds;
		}

		private async void TimerEnded(Object source, ElapsedEventArgs args)
		{
			cleanupTimer.Interval = 8.64e+7;
			cleanupTimer.Start();
			await RunCleanup();
		}
		private async Task RunCleanup()
		{

			await DataManager.RunCleanup(config.MessageTimeToDie);
		}


		private async Task HandleMessageDeleteAsync(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
		{
			ulong guildID = ((SocketTextChannel)channel).Guild.Id;
			LoggedMessage logMsg = await MessageLogger.GetLog(message.Id, guildID);	

			var embed = new EmbedBuilder();
			var em = embed.AddField("Message " + logMsg.MessageID.ToString(), logMsg.MessageContent)
				.WithAuthor(_client.GetGuild(guildID).GetUser(logMsg.AuthorID))
				.WithColor(Color.Red)
				.WithFooter(footer => footer.Text = "ArbitiesLog")
				.WithTimestamp(logMsg.Timestamp)
				.Build();
			GuildData guildData = await GuildManager.GetGuildData(guildID);
			await ((SocketTextChannel)_client.GetChannel(guildData.LogChannel)).SendMessageAsync(embed: em);
		}





		private async Task HandleCommandAsync(SocketMessage arg)
		{
			if (arg.Author == _client.CurrentUser) return;
			await MessageLogger.LogMessage(arg);
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

		public static Task Log(LogMessage message)
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

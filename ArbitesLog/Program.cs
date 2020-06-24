using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace ArbitesLog
{
	class Program
	{
		public static void Main() => new Program().MainAsync().GetAwaiter().GetResult();
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;
		private readonly Config config;
		private Timer cleanupTimer;
		private CommandHandler commandHandler;

		public async Task MainAsync()
		{
			commandHandler = new CommandHandler(_client, _commands, config);
			await commandHandler.InitCommands();
			InitLogger();
			InitCleaner();

			await _client.LoginAsync(TokenType.Bot, config.Token);
			await _client.StartAsync();

			// Block this task until the program is closed.
			await Task.Delay(-1);
		}

		private Program()
		{
			config = Startup.ConfigStart();
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
		}

		private void InitLogger()
		{
			_client.MessageDeleted += HandleMessageDeleteAsync;
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

		private async void TimerEnded(object source, ElapsedEventArgs args)
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

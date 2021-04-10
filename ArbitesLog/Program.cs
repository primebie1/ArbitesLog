using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;
using System.Collections;
using System.Text;
using System.Reflection;

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
			Console.WriteLine($"ArbitesLog v{Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}");
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
			_client.MessageUpdated += _client_MessageUpdated;
			_client.UserJoined += _client_UserJoined;
			_client.UserLeft += _client_UserLeft;
			_client.UserBanned += _client_UserBanned;
			_client.UserUnbanned += _client_UserUnbanned;
			_client.UserUpdated += _client_UserUpdated;
			_client.MessagesBulkDeleted += _client_MessagesBulkDeleted;
		}

		private async Task _client_MessagesBulkDeleted(System.Collections.Generic.IReadOnlyCollection<Cacheable<IMessage, ulong>> messages, ISocketMessageChannel channel)
		{
			ulong guildID = ((SocketTextChannel)channel).Guild.Id;
			StringBuilder fieldValue = new StringBuilder();
			foreach (Cacheable<IMessage, ulong> cached in messages) 
			{
				fieldValue.Append(cached.Id + ", ");
			}

			var origField = new EmbedFieldBuilder()
				.WithName(messages.Count + " Messages Deleted")
				.WithValue(fieldValue.ToString());
			var footer = new EmbedFooterBuilder()
				.WithIconUrl(_client.CurrentUser.GetAvatarUrl())
				.WithText("ArbitiesLog");
			var embed = new EmbedBuilder()
				.AddField(origField)
				.WithFooter(footer)
				.WithColor(Color.Red)
				.Build();
			GuildData guildData = await GuildManager.GetGuildData(guildID);
			await((SocketTextChannel)_client.GetChannel(guildData.LogChannel)).SendMessageAsync(embed: embed);
		}

		private async Task _client_UserUpdated(SocketUser arg1, SocketUser arg2)
		{
			ulong guildID = ((SocketGuildUser)arg1).Guild.Id;
			LogMessage msg = new LogMessage(LogSeverity.Info, "EventLogger", $"User {arg1.Id} in Guild {guildID} Updated!");
			Log(msg);
			SocketGuildUser userStart = (SocketGuildUser)arg1;
			SocketGuildUser userEnd = (SocketGuildUser)arg2;

			string actionTaken = "Should Not Be Seen";
			StringBuilder fieldValue = new StringBuilder();
			if (userStart.Roles.Count != userEnd.Roles.Count)
			{
				if (userStart.Roles.Count < userEnd.Roles.Count)
				{
					//Role Added
					IEnumerable differentRoles = userEnd.Roles.Except(userStart.Roles);
					actionTaken = "Role(s) Added";
					foreach (SocketRole role in differentRoles)
					{
						fieldValue.Append(role.Name + "\n");
					}
				}
				else
				{
					//Role Removed
					IEnumerable differentRoles = userStart.Roles.Except(userEnd.Roles);
					actionTaken = "Role(s) Removed";
					foreach (SocketRole role in differentRoles)
					{
						fieldValue.Append(role.Name + "\n");
					}
				}
			}
			else if (userStart.Nickname != userEnd.Nickname)
			{
				actionTaken = "Nickname Changed";
				fieldValue.Append(userStart.Nickname + " --> " + userEnd.Nickname);
			}

			var origField = new EmbedFieldBuilder()
				.WithName(actionTaken)
				.WithValue(fieldValue.ToString());
			var footer = new EmbedFooterBuilder()
				.WithIconUrl(_client.CurrentUser.GetAvatarUrl())
				.WithText("ArbitiesLog");
			var embed = new EmbedBuilder()
				.WithImageUrl(userStart.GetAvatarUrl(ImageFormat.Auto, 512))
				.AddField(origField)
				.WithFooter(footer)
				.WithColor(Color.Gold)
				.Build();
			GuildData guildData = await GuildManager.GetGuildData(guildID);
			await ((SocketTextChannel)_client.GetChannel(guildData.LogChannel)).SendMessageAsync(embed: embed);
		}

		private async Task _client_UserUnbanned(SocketUser user, SocketGuild guild)
		{
			ulong guildID = guild.Id;
			LogMessage msg = new LogMessage(LogSeverity.Info, "EventLogger", $"User {user.Id} in Guild {guildID} Unbanned!");
			Log(msg);

			var origField = new EmbedFieldBuilder()
				.WithName("User Unbanned")
				.WithValue(user.Username);
			var footer = new EmbedFooterBuilder()
				.WithIconUrl(_client.CurrentUser.GetAvatarUrl())
				.WithText("ArbitiesLog");
			var embed = new EmbedBuilder()
				.WithImageUrl(user.GetAvatarUrl(ImageFormat.Auto, 512))
				.AddField(origField)
				.WithFooter(footer)
				.WithColor(Color.Green)
				.Build();
			GuildData guildData = await GuildManager.GetGuildData(guildID);
			await((SocketTextChannel)_client.GetChannel(guildData.LogChannel)).SendMessageAsync(embed: embed);
		}

		private async Task _client_UserBanned(SocketUser user, SocketGuild guild)
		{
			ulong guildID = guild.Id;
			LogMessage msg = new LogMessage(LogSeverity.Info, "EventLogger", $"User {user.Id} in Guild {guildID} Banned!");
			Log(msg);

			var origField = new EmbedFieldBuilder()
				.WithName("User Banned")
				.WithValue(user.Username);
			var footer = new EmbedFooterBuilder()
				.WithIconUrl(_client.CurrentUser.GetAvatarUrl())
				.WithText("ArbitiesLog");
			var embed = new EmbedBuilder()
				.WithImageUrl(user.GetAvatarUrl(ImageFormat.Auto, 512))
				.AddField(origField)
				.WithFooter(footer)
				.WithColor(Color.Red)
				.Build();
			GuildData guildData = await GuildManager.GetGuildData(guildID);
			await((SocketTextChannel)_client.GetChannel(guildData.LogChannel)).SendMessageAsync(embed: embed);
		}

		private async Task _client_UserLeft(SocketGuildUser user)
		{
			ulong guildID = user.Guild.Id;
			LogMessage msg = new LogMessage(LogSeverity.Info, "EventLogger", $"User {user.Id} Left Guild {guildID}!");
			Log(msg);

			var origField = new EmbedFieldBuilder()
				.WithName("User Left")
				.WithValue(user.Username);
			var footer = new EmbedFooterBuilder()
				.WithIconUrl(_client.CurrentUser.GetAvatarUrl())
				.WithText("ArbitiesLog");
			var embed = new EmbedBuilder()
				.WithImageUrl(user.GetAvatarUrl(ImageFormat.Auto, 512))
				.AddField(origField)
				.WithFooter(footer)
				.WithColor(Color.Red)
				.Build();
			GuildData guildData = await GuildManager.GetGuildData(guildID);
			await ((SocketTextChannel)_client.GetChannel(guildData.LogChannel)).SendMessageAsync(embed: embed);
		}

		private async Task _client_UserJoined(SocketGuildUser user)
		{
			ulong guildID = user.Guild.Id;
			LogMessage msg = new LogMessage(LogSeverity.Info, "EventLogger", $"User {user.Id} Joined Guild {guildID}!");
			Log(msg);

			var creationTime = user.CreatedAt.UtcDateTime;
			TimeSpan timeExisted = DateTimeOffset.UtcNow - creationTime;
			var origField = new EmbedFieldBuilder()
				.WithName("New User Joined")
				.WithValue(user.Username);
			var timeField = new EmbedFieldBuilder()
				.WithName("Account Age")
				.WithValue(timeExisted.ToString());
			var footer = new EmbedFooterBuilder()
				.WithIconUrl(_client.CurrentUser.GetAvatarUrl())
				.WithText("ArbitiesLog");
			var embed = new EmbedBuilder()
				.WithImageUrl(user.GetAvatarUrl(ImageFormat.Auto, 512))
				.AddField(origField)
				.AddField(timeField)
				.WithFooter(footer)
				.WithColor(Color.Green)
				.Build();
			GuildData guildData = await GuildManager.GetGuildData(guildID);
			await((SocketTextChannel)_client.GetChannel(guildData.LogChannel)).SendMessageAsync(embed: embed);
		}

		private async Task _client_MessageUpdated(Cacheable<IMessage, ulong> origMsg, SocketMessage message, ISocketMessageChannel channel)
		{
			ulong guildID = ((SocketTextChannel)channel).Guild.Id;
			LoggedMessage logMsg = await MessageLogger.GetLog(origMsg.Id, guildID);


			var origField = new EmbedFieldBuilder()
				.WithName("Original Text of " + logMsg.MessageID.ToString())
				.WithValue(logMsg.MessageContent[^1])
				.WithIsInline(true);
			var newField = new EmbedFieldBuilder()
				.WithName("New Text Of " + logMsg.MessageID.ToString())
				.WithValue(message.Content)
				.WithIsInline(true);
			var footer = new EmbedFooterBuilder()
				.WithIconUrl("https://cdn.discordapp.com/avatars/729417603625517198/254497db41bcb2143e7b69274a857bda.png")
				.WithText("ArbitiesLog");

			var embed = new EmbedBuilder();
			var em = embed.AddField(origField)
				.AddField(newField)
				.WithAuthor(_client.GetGuild(guildID).GetUser(logMsg.AuthorID))
				.WithColor(Color.Gold)
				.WithFooter(footer)
				.WithTimestamp(logMsg.Timestamp)
				.Build();
			logMsg.MessageContent.Add(message.Content);
			MessageLogger.UpdateLog(origMsg.Id, guildID, logMsg);
			GuildData guildData = await GuildManager.GetGuildData(guildID);
			await ((SocketTextChannel)_client.GetChannel(guildData.LogChannel)).SendMessageAsync(embed: em);

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

			var msgField = new EmbedFieldBuilder()
				.WithName("Message " + logMsg.MessageID.ToString())
				.WithValue(logMsg.MessageContent[^1])
				.WithIsInline(true);
			var footer = new EmbedFooterBuilder()
				.WithIconUrl("https://cdn.discordapp.com/avatars/729417603625517198/254497db41bcb2143e7b69274a857bda.png")
				.WithText("ArbitiesLog");

			var embed = new EmbedBuilder();
			var em = embed.AddField(msgField)
				.WithAuthor(_client.GetGuild(guildID).GetUser(logMsg.AuthorID))
				.WithColor(Color.Red)
				.WithFooter(footer)
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

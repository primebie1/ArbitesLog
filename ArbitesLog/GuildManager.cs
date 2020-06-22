using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using System.IO;
using System.Text.Json;

namespace ArbitesLog
{
	public class GuildManager
	{
		static public async Task<GuildData> GetGuildData(ulong guildID)
		{

			string jsonIn = await File.ReadAllTextAsync("Config\\" + guildID + ".json");
			return JsonSerializer.Deserialize<GuildData>(jsonIn);
		}
		static public bool CheckGuildData(ulong guildID)
		{
			if (!Directory.Exists("Config\\"))
			{
				Directory.CreateDirectory("Config\\");
			}
			return File.Exists("Config\\" + guildID + ".json");
		}

		static public async Task SetGuildData(GuildData data)
		{
			JsonSerializerOptions options = new JsonSerializerOptions
			{
				WriteIndented = true
			};
			string jsonOut = JsonSerializer.Serialize(data, options);
			await File.WriteAllTextAsync("Config\\" + data.GuildID + ".json", jsonOut);
		}
	}

	/// <summary>
	/// Data For Managing Per-Guild Configuration
	/// </summary>
	public class GuildData
	{
		public ulong GuildID { get; set; }
		public ulong LogChannel { get; set; }
	}
}

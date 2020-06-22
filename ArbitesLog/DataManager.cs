using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Discord;


namespace ArbitesLog
{
	public class DataManager
	{
		static public async Task RunCleanup(int messageTimeToDie)
		{
			LogMessage log = new LogMessage(LogSeverity.Info, "Cleanup", "Running Cleanup...");
			await Program.Log(log);
			string[] guilds = Directory.GetDirectories("Logs\\");
			foreach (string guild in guilds)
			{
				string[] messages = Directory.GetFiles(guild);
				foreach(string message in messages)
				{
					TimeSpan span = DateTime.Now - File.GetCreationTime(message);
					if (span.TotalDays > messageTimeToDie)
					{
						File.Delete(message);
					}

				}
			}
			log = new LogMessage(LogSeverity.Info, "Cleanup", "Cleanup Complete!");
			await Program.Log(log);

		}
	}
}

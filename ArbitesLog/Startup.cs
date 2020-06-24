using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.Json;

namespace ArbitesLog
{
	public class Startup
	{
		static readonly string confFile = "Config\\config.json";
		static public Config ConfigStart()
		{
			if (File.Exists(confFile))
			{
				return ConfLoad();

			}
			else
			{
				return ConfSetup();
			}
		}

		//First Time Setup
		static Config ConfSetup()
		{
			Config config = new Config();
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
			return config;
		}

		static Config ConfLoad()
		{
			
			//load master config
			string jsonstring = File.ReadAllText(confFile);
			return JsonSerializer.Deserialize<Config>(jsonstring);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ArbitesLog
{
	/// <summary>
	/// Config Data For Operation Of ArbitesLog
	/// </summary>
	class Config
	{
		public string Token { get; set; }
		public ulong LogChannel { get; set; }
		public char Prefix { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ArbitesLog
{
	/// <summary>
	/// Class for storing message data long term
	/// </summary>
	public class LoggedMessage
	{
		public ulong MessageID { get; set; }
		public List<string> MessageContent { get; set; }
		public ulong AuthorID { get; set; }
		public ulong ChannelID { get; set; }
		public DateTimeOffset Timestamp { get; set; }
	}
}

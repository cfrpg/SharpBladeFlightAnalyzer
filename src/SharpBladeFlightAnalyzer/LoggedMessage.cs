using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBladeFlightAnalyzer
{
	public class LoggedMessage
	{
		LogLevel level;
		ulong timestamp;
		string message;

		public LogLevel Level
		{
			get { return level; }
			set { level = value; }
		}

		public ulong Timestamp
		{
			get { return timestamp; }
			set { timestamp = value; }
		}

		public string Message
		{
			get { return message; }
			set { message = value; }
		}
	}

	public enum LogLevel:byte
	{
		EMERGENCY = 0x30,
		ALERT = 0x31,
		CRITICAL = 0x32,
		ERROR = 0x33,
		WARNING = 0x34,
		NOTICE = 0x35,
		INFO = 0x36,
		DEBUG = 0x37
	}
}

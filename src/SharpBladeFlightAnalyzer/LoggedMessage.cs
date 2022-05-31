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
		DateTime time;
		LogTag tag;

		public LogLevel Level
		{
			get { return level; }
			set { level = value; }
		}

		public ulong Timestamp
		{
			get { return timestamp; }
			set
			{
				timestamp = value;
				time = ULogFile.UnixStartTime.AddMilliseconds(timestamp / 1000);
			}
		}

		public string Message
		{
			get { return message; }
			set { message = value; }
		}

		public DateTime Time
		{
			get { return time; }
		}

		public LogTag Tag { get => tag; set => tag = value; }

		public string TagString
		{
			get
			{
				if (Tag != LogTag.notag)
					return tag.ToString();
				return "";
			}
		}

		public LoggedMessage()
		{
			tag = LogTag.notag;
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

	public enum LogTag : ushort
	{
		unassigned,
		mavlink_handler,
		ppk_handler,
		camera_handler,
		ptp_handler,
		serial_handler,
		watchdog,
		io_service,
		cbuf,
		ulg,
		notag=9999
	}
}

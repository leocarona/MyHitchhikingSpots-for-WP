using System;
using SQLite;

namespace MyHitchhikingSpots.Models
{
	public class LogEntry
	{
		/// <summary>
		/// Is set in the constructor.
		/// </summary>
		public string ID { get; set; }
		public string Method { get; set; }
		public string ExceptionMessage { get; set; }
		public string InnerExceptionMessage { get; set; }
		public string StackTrace { get; set; }
		public string InnerStackTrace { get; set; }
		/// <summary>
		/// Is set in the constructor
		/// </summary>
		public DateTime DateTime { get; set; }
		public string Message { get; set; }
		public string TimeSpan { get; set; }
		public LogType LogType { get; set; }



		public LogEntry()
		{
			DateTime = DateTime.Now;
			ID = Guid.NewGuid().ToString();
		}
	}


	public enum LogType
	{
		Failed,
		Warning,
		Information,
		Success
	}
}


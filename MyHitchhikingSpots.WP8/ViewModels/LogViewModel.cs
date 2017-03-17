using System;
//using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MyHitchhikingSpots.Models;
using MyHitchhikingSpots.Tools;
using MyHitchhikingSpots.Databases;


#if NOLINEA
using LB.Shared;
#endif



namespace MyHitchhikingSpots.ViewModels
{
	public class LogViewModel : ViewModelBase
	{
		readonly PrivateDB db;

		public LogViewModel ()
		{
			db = ServiceContainer.Resolve<PrivateDB> ();
		}

		public LogEntry SelectedLogEntry { get; set; }



		public string CreateLogEntry (string Message, LogType logType, string methodName = "")
		{
			var logEntry = new LogEntry () { Message = Message, LogType = logType, Method = methodName };
			SaveToLog (logEntry);
			return logEntry.ID;
		}

		public string CreateLogEntry (string Message, Exception exception, string methodName = "", string timespan = "")
		{
			var logEntry = new LogEntry () {
				Message = Message,
				LogType = LogType.Failed,
				ExceptionMessage = exception.Message,
				StackTrace = exception.StackTrace,
				InnerStackTrace = (exception.InnerException != null) ? exception.InnerException.StackTrace : "",
				InnerExceptionMessage = (exception.InnerException != null) ? exception.InnerException.Message : "",
				Method = methodName,
				TimeSpan = timespan
			};
			SaveToLog (logEntry);

			return logEntry.ID;
		}


		public void SaveToLog (LogEntry logEntry)
		{
#if DEBUG
            System.Diagnostics.Debug.WriteLine("+ " + logEntry.Message + "\n++ Method name:" + logEntry.Method);
#endif

			db.GetConnection ().InsertAsync (logEntry);
		}


		public Task<List<LogEntry>> GetLogs ()
		{
			return db.GetConnection ().Table<LogEntry> ().OrderByDescending (s => s.DateTime).ToListAsync ();
		}

		public Task<LogEntry> GetLog (string logId)
		{
			return db.GetConnection ().Table<LogEntry> ().Where (i => i.ID == logId).FirstOrDefaultAsync ();
		}

	}
}
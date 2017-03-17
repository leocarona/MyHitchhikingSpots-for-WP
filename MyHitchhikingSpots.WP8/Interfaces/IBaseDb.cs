using System;
using System.Threading.Tasks;
using System.Threading;
using SQLite;

namespace MyHitchhikingSpots.Models
{
	public interface IBaseDb
	{
        void Initialize();
        //Task Initialize (CancellationToken cancellationToken);
        SQLiteAsyncConnection GetConnection();
        String DBFileName { get; }
        String DBFilePath { get; }
        Type[] TableTypes { get; }
	}
}


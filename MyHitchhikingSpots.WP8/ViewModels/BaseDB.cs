using MyHitchhikingSpots.Tools;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyHitchhikingSpots.Models
{
    public abstract class BaseDB : IBaseDb
    {
        protected bool initialized = false;

        public abstract String DBFileName { get; }

        public abstract Type[] TableTypes { get; }

        public String DBFilePath
        {
            get
            {
#if NETFX_CORE
         return DBFileName;  //TODO: Who use NETFX_CORE should check if this is the expected value to be returned
#elif NCRUNCH 
        return System.IO.Path.GetTempFileName(); 
#else
                return System.IO.Path.Combine(Tool.DocumentsPath, DBFileName);
#endif
            }
        }

        public void CloseConnection()
        {
            var c = GetConnection();
            if (c != null)
                c.CloseConnection();
        }
        /// <summary>
        /// For use within the app on startup, this will create the database
        /// </summary>
        /// <returns></returns>
        public void Initialize()
        {
            CreateDatabase(new SQLiteAsyncConnection(DBFilePath, true), TableTypes);
        }
  

        /// <summary>
        /// Global way to grab a connection to the database, make sure to wrap in a using
        /// </summary>
        public SQLiteAsyncConnection GetConnection()
        {
            var connection = new SQLiteAsyncConnection(DBFilePath, true);
            if (!initialized)
            {
                CreateDatabase(connection);
            }
            return connection;
        }


        void CreateDatabase(SQLiteAsyncConnection connection, params Type[] types)
        {
            //Create the tables
            var createTask = connection.CreateTablesAsync(TableTypes);
            createTask.Wait();
            
            ////Count number of assignments
            //var countTask = connection.Table<MenuCategory>().CountAsync();
            //countTask.Wait();

            ////If no assignments exist, insert our initial data
            //if (countTask.Result == 0)
            //{
            //    var insertTask = connection.InsertAllAsync(TestDataBusinessMenu.All);

            //    //Wait for inserts
            //    insertTask.Wait();
            //}

            //Mark database created
            initialized = true;
        }
    }

}

using System;
using MyHitchhikingSpots.Models;
using System.Threading.Tasks;
using SQLite;
using System.Threading;
using MyHitchhikingSpots.Tools;
//using Xamarin.Geolocation;

namespace MyHitchhikingSpots.Databases
{
    public class PrivateDB : BaseDB
    {
        public override String DBFileName { get { return MyHitchhikingSpots.Tools.Constants.PrivateDbFileName; } }

        public override Type[] TableTypes
        {
            get
            {
                return new Type[]
		        {
			        typeof(Preference),
                    typeof(LogEntry), 

                    typeof(LocationHolder)

		        };
            }
        }
    }
}


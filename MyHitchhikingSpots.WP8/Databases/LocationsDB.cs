using System;
//using MobiCMS.Data;
using System.Threading;
using System.Threading.Tasks;
using SQLite;
using MyHitchhikingSpots.Models;

namespace MyHitchhikingSpots.Databases
{
    public class LocationsDB : BaseDB
    {
        public override String DBFileName { get { return MyHitchhikingSpots.Tools.Constants.LocationsDbFileName; } }

        public override Type[] TableTypes
        {
            get
            {
                return new Type[]
		        {
                    typeof(Map),
			        typeof(MapItem),
		        };
            }
        }
    }
}


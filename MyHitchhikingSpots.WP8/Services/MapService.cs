using System;
using System.Threading.Tasks;
using MyHitchhikingSpots.Databases;
using System.Collections.Generic;
using System.Threading;
using MyHitchhikingSpots.Interfaces;
using System.Linq;
using MyHitchhikingSpots.Models;
using MyHitchhikingSpots.Tools;


namespace MyHitchhikingSpots.Services
{
    public class MapService : IMapService
    {
        #region IMapService implementation

         LocationsDB db;

        public MapService()
        {
            db = ServiceContainer.Resolve<LocationsDB>();
        }

        public string GetDBFileName() { return db.DBFileName; }
        public void CloseConnection()
        {
            db.CloseConnection();
        }

        public void OpenNewConnection() {
            ServiceContainer.Register<LocationsDB>(()=>new LocationsDB());
            db = ServiceContainer.Resolve<LocationsDB>();
        }

        public Task<int> SaveMapAsync(Map map)
        {
            return db.GetConnection().InsertAsync(map);
        }

        public Task<int> UpdateMapAsync(Map map)
        {
            return db.GetConnection().UpdateAsync(map);
        }

        public Task<int> DeleteMapAsync(Map map)
        {
            return db.GetConnection().DeleteAsync(map);
        }


        public Task<Map> GetMapAsync(int mapId)
        {
            return db.GetConnection()
             .Table<Map>()
             .Where(i => i.Id == mapId)
             .FirstOrDefaultAsync();
        }

        public Task<List<Map>> GetAllMapsAsync()
        {
            return db.GetConnection()
                    .Table<Map>()
                    .OrderBy(i => i.Name)
                    .ToListAsync();
        }

        public Task<Map> GetLastMapCreatedAsync()
        {
            return db.GetConnection()
                    .Table<Map>()
                    .OrderByDescending(i => i.DateTime)
                    .FirstOrDefaultAsync();
        }

        public Task<int> SaveMapItemAsync(IMapItem mapItem)
        {
            return db.GetConnection().InsertAsync(mapItem);
        }

        public Task<int> UpdateMapItemAsync(IMapItem mapItem)
        {
            return db.GetConnection().UpdateAsync(mapItem);
        }

        public Task<int> DeleteMapItemAsync(IMapItem mapItem)
        {
            return db.GetConnection().DeleteAsync(mapItem);
        }

        public Task<List<MapItem>> GetMapItemsAsync()
        {
            return db.GetConnection()
                    .Table<MapItem>()
                    //.OrderBy(i => i.Name)
                    .ToListAsync();

        }



        #endregion


    }
}


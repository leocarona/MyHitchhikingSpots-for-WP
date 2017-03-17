using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MyHitchhikingSpots.Models;
using System.Threading;
using MyHitchhikingSpots.Interfaces;

namespace MyHitchhikingSpots.Interfaces
{
    public interface IMapService
    {
        Task<int> SaveMapAsync(Map map);
        Task<int> UpdateMapAsync(Map map);
        Task<int> DeleteMapAsync(Map map);

        Task<List<Map>> GetAllMapsAsync();
        Task<Map> GetMapAsync(int mapId);
        Task<Map> GetLastMapCreatedAsync();

        /// <summary>
        /// Gets a list of menu items
        /// </summary>
        Task<List<MapItem>> GetMapItemsAsync();

        Task<int> SaveMapItemAsync(IMapItem mapItem);
        Task<int> UpdateMapItemAsync(IMapItem mapItem);
        Task<int> DeleteMapItemAsync(IMapItem mapItem);
    }
}


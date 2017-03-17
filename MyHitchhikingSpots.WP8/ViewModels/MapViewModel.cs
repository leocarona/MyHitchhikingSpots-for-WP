using System;
using MyHitchhikingSpots.ViewModels;
using System.Collections.Generic;
using MyHitchhikingSpots.Interfaces;
//using MyHitchhikingSpots.Utilities;
using System.Linq;
using System.Threading.Tasks;
//using MobiCMS;
using MyHitchhikingSpots.Models;
using MyHitchhikingSpots.Tools;
using System.Globalization;
using MyHitchhikingSpots.Services;

//#if WINDOWS_PHONE
//using MyHitchhikingSpots.Resources;
//#endif



namespace MyHitchhikingSpots.ViewModels
{
    public partial class MapViewModel : ViewModelBase
    {
        readonly MapService service;

        List<Map> _maps;

        public List<Map> Maps
        {
            get { return _maps; }
            set { _maps = value; Tool.RunInUIThread(()=> OnPropertyChanged("Maps")); }
        }

        Map _selectedMap;

        public Map SelectedMap
        {
            get { return _selectedMap; }
            set { _selectedMap = value;Tool.RunInUIThread(()=> OnPropertyChanged("SelectedMap")); }
        }

        public MapViewModel()
        {
            service = ServiceContainer.Resolve<MapService>();


            //            locViewModel.OnCurrentPositionChanged += (e, s) =>
            //            {
            //                _mapItemList = CalculateDistance(_mapItemList);

            //#if WINDOWS_PHONE
            //                //Update the map item list with the new distances on the UI.
            //                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() => OnPropertyChanged("MapItemList"));
            //#endif
            //            };

        }

        public string GetDBFileName() { return service.GetDBFileName(); }

        public void CloseConnection()
        {
            service.CloseConnection();
        }

        public void OpenNewConnection()
        {
            service.OpenNewConnection();
        }

        public Task<List<MapItem>> LoadMapItemsAsync()
        {
            return service.GetMapItemsAsync();

            //On Windows Phone CalculateDistance is also called always the the current position is updated.
            //t = CalculateDistance(t);

        }




        public Task SaveMap(Map map)
        {
            //Save the map item
            return service.SaveMapAsync(map);
        }

        public Task UpdateMap(Map map)
        {
            //Update the map item
            return service.UpdateMapAsync(map);
        }

        public Task DeleteMap(Map map)
        {
            //Save the map item
            return service.DeleteMapAsync(map);
        }

        public Task<int> SaveMapItem(IMapItem mapItem)
        {
            //Save the map item
            return service.SaveMapItemAsync(mapItem);
        }

        public Task<int> UpdateMapItem(IMapItem mapItem)
        {
            //Update the map item
            return service.UpdateMapItemAsync(mapItem);
        }

        public Task DeleteMapItem(IMapItem mapItem)
        {
            //Save the map item
            return service.DeleteMapItemAsync(mapItem);
        }




        List<MapItem> _mapItemList;

#if WINDOWS_PHONE
        public int DefaultMapZoomLevel { get { return Constants.DefaultStartMapsZoomLevel; } }
#endif

        /// <summary>
        /// List of MapItems for the selected map.
        /// </summary>
        public List<MapItem> MapItemList
        {
            get
            {
                try
                {
                    if (_mapItemList == null)
                        _mapItemList = new List<MapItem>();

                    var result = _mapItemList;

                    if (SelectedMap != null && SelectedMap.Id > 0)
                        result = result.Where(p => p.MapId == SelectedMap.Id).ToList();

                    return result.ToList();//.OrderByDescending(i => i.DateTime)
                }
                catch (Exception ex)
                {
                    Tool.WriteExceptionMessagesToOutputBox(ex);
                    return new List<MapItem>();
                }
            }
        }



        bool _loadingMapItems;

        public bool LoadingMapItems
        {
            get { return _loadingMapItems; }
            set { _loadingMapItems = value; OnPropertyChanged("LoadingMapItems"); }
        }



        internal async Task LoadAllMaps()
        {
            try
            {
                Maps = await service.GetAllMapsAsync();
            }
            catch (Exception ex)
            {
                Tool.WriteExceptionMessagesToOutputBox(ex);
                Maps = new List<Map>();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapId">-1 to read the last map created, 0 to read all the maps or a valid map Id.</param>
        /// <param name="spotsType"></param>
        /// <returns></returns>
        public async Task LoadMap(int mapId = -1)
        {
            try
            {
                //if (mapId > 0)
                //{
                //    SelectedMap = service.GetMapAsync(mapId).Result;
                //}
                //else if (mapId == -1 && isFirstLoad)
                //{
                //SelectedMap = service.GetLastMapCreatedAsync().Result;
                isFirstLoad = false;
                //}

                await LoadMapItems();
            }
            catch (Exception ex)
            {
                Tool.WriteExceptionMessagesToOutputBox(ex);
            }
        }

        bool isFirstLoad = true;

        /// <summary>
        /// Loads the map items
        /// </summary>
        /// <param name="spotsType">The type of the spots that must be shown. Inform SpotType.Unknown to show all spots available in the database.</param>
        /// <returns></returns>
        async Task LoadMapItems()
        {

            try
            {
                LoadingMapItems = true;

                var t = await LoadMapItemsAsync();
                if (t.Any())
                {
                    SelectLastItem(t);
                    _mapItemList = t;
                    OnPropertyChanged("MapItemList");
                }
            }
            catch (Exception ex)
            {
                Tool.WriteExceptionMessagesToOutputBox(ex);
            }

            LoadingMapItems = false;

        }

        private void SelectLastItem(List<MapItem> mapItems)
        {
            if (mapItems != null && mapItems.Count() > 0)
            {
                var newSpots = mapItems.Where(s => s.AttemptResult == AttemptResult.Unknown);//.OrderByDescending(i => i.DateTime)

                if (newSpots.Any())
                    SelectedMapItem = newSpots.FirstOrDefault();
            }
        }





        MapItem _SelectedMapItem;

        public MapItem SelectedMapItem
        {
            get { return _SelectedMapItem; }
            set { _SelectedMapItem = value; OnPropertyChanged("SelectedMapItem"); }
        }

        public void SelectMap(int mapId)
        {
            SelectedMap = Maps.Where(p => p.Id == mapId).SingleOrDefault();
        }

        public void UnselectMap()
        {
            SelectedMap = null;
        }

        public void SelectMapItem(int mapItemId)
        {
            SelectedMapItem = MapItemList.Where(p => p.Id == mapItemId).SingleOrDefault();
        }

        public void UnselectMapItem()
        {
            SelectedMapItem = null;
        }

        public Task SaveSpot(string spotName, string note, Hitchability hitchability, DateTime date)
        {
            //int newId = MapItemList.Count();

            //Id = newId,
            SelectedMapItem.Name = spotName;
            SelectedMapItem.Note = note;
            SelectedMapItem.Hitchability = hitchability;
            SelectedMapItem.DateTime = date;
            //SelectedMapItem.City = locViewModel.City;
            //SelectedMapItem.Country = locViewModel.Country;
            //SelectedMapItem.Longitude = locViewModel.CurrentPosition.Longitude;
            //SelectedMapItem.Latitude = locViewModel.CurrentPosition.Latitude;

            if (SelectedMap != null)
                SelectedMapItem.MapId = SelectedMap.Id;
            else
                SelectedMapItem.MapId = -1;

            return SaveSpot(SelectedMapItem);
        }

        private Task SaveSpot(MapItem objToSave)
        {
            return SaveMapItem(objToSave).ContinueWith(s =>
            {
                SelectedMapItem = objToSave;

                _mapItemList.Add(objToSave);
                Tool.RunInUIThread(() => OnPropertyChanged("MapItemList"));
            });
        }

        public async Task DeleteSpot(MapItem mapItem)
        {
            try
            {
                await DeleteMapItem(mapItem);

                var index = _mapItemList.IndexOf(mapItem);

                if (index >= 0)
                    _mapItemList.RemoveAt(index);

                Tool.RunInUIThread(() => OnPropertyChanged("MapItemList"));

                //Update selected item 
                SelectLastItem(MapItemList);
            }
            catch (Exception ex)
            {
                Tool.WriteExceptionMessagesToOutputBox(ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapItem"></param>
        /// <param name="waitingTime">Waiting time in minutes</param>
        /// <param name="attemptResult"></param>
        public async Task EvaluateSpot(MapItem mapItem, int waitingTime, AttemptResult attemptResult, int mapId)
        {
            mapItem.WaitingTime = TimeSpan.FromMinutes(waitingTime);
            mapItem.AttemptResult = attemptResult;
            mapItem.MapId = mapId;

            await UpdateMapItem(mapItem)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        MyHitchhikingSpots.Tools.Tool.WriteExceptionMessagesToOutputBox(t.Exception);
                });

            UnselectMapItem();
        }

        public async Task UpdateSpot(MapItem mapItem, string spotName, string note, Hitchability hitchability, DateTime date, int waitingTime, AttemptResult attemptResult, int mapId)
        {
            mapItem.Name = spotName;
            mapItem.Note = note;
            mapItem.Hitchability = hitchability;
            mapItem.DateTime = date;

            mapItem.WaitingTime = TimeSpan.FromMinutes(waitingTime);
            mapItem.AttemptResult = attemptResult;
            mapItem.MapId = mapId;

            await UpdateMapItem(mapItem)
                 .ContinueWith(t =>
                 {
                     if (t.IsFaulted)
                         MyHitchhikingSpots.Tools.Tool.WriteExceptionMessagesToOutputBox(t.Exception);
                 });

            UnselectMapItem();
        }

        public int GetWaitingTime()
        {
            int waitingTime = 0;
            try
            {
                if (SelectedMapItem.GetDataState() == SpotDataState.BasicInfoInserted)
                {
                    if (SelectedMapItem.DateTime != default(DateTime))
                    {
                        var waitingTimeSpan = DateTime.Now - SelectedMapItem.DateTime;
                        waitingTime = (int)waitingTimeSpan.TotalMinutes;
                    }
                }
            }
            catch (Exception ex)
            {
                Tool.WriteExceptionMessagesToOutputBox(ex);
            }
            return waitingTime;
        }

        public string GetRouteUrl()
        {
            try
            {
                var mapUrl = "";

                foreach (var p in MapItemList)//.OrderBy(i => i.DateTime)
                    mapUrl += string.Format("/{0},{1}", p.Latitude.ToString(new CultureInfo("es-ES")), p.Longitude.ToString(new CultureInfo("es-ES")));

                if (!String.IsNullOrEmpty(mapUrl))
                    return "https://www.google.com/maps/dir" + mapUrl;
            }
            catch (Exception ex)
            {
                Tool.WriteExceptionMessagesToOutputBox(ex);
            }
            return "";
        }




        public void ReverseGeocodeSelectedMapItem()
        {
            ReverseGeocode(SelectedMapItem.Id, SelectedMapItem.Latitude, SelectedMapItem.Longitude);
        }


        private void ReverseGeocode(int mapItemid, double latitude, double longitude)
        {
            try
            {
                bool isOnline = false;
#if ANDROID
                isOnline=true;
#else
                isOnline=Tool.IsOnline();
#endif
                if (!isOnline)
                    throw new Exception("You don't seem to have an active internet connection at the momment. Please, try again later.");
                _mapItemIdWaitingReverseGeocode = mapItemid;
                var uri = Tool.GetReverseGeocoderUri(latitude, longitude);
                GeoCoder.ReverseGeocodeAsync(uri, async x => await ReverseGeocodeCompleted(x));
            }
            catch (Exception ex)
            {
                Tools.Tool.WriteExceptionMessagesToOutputBox(ex);
                if (CallFinished != null)
                    CallFinished(false);
            }
        }

        int _mapItemIdWaitingReverseGeocode = -1;
        bool IsWaitingReverseGeocodeResult(int mapItemId)
        {
            return (_mapItemIdWaitingReverseGeocode == mapItemId);
        }

        async Task ReverseGeocodeCompleted(Dictionary<string, string> result)
        {
            bool sucess = false;

            try
            {
                _mapItemIdWaitingReverseGeocode = -1;
                if (result != null && SelectedMapItem != null && IsWaitingReverseGeocodeResult(SelectedMapItem.Id))
                {
                    foreach (var i in result)
                    {
                        switch (i.Key)
                        {
                            case "country":
                                SelectedMapItem.Country = result["country"];
                                break;
                            case "locality":
                                SelectedMapItem.City = result["locality"];
                                break;
                            case "administrative_area_level_2":
                                SelectedMapItem.State = result["administrative_area_level_2"];
                                break;
                            case "route":
                                SelectedMapItem.Street = result["route"];
                                break;
                            case "postal_code":
                                SelectedMapItem.Zip = result["postal_code"];
                                break;
                        }
                    }

                    await UpdateMapItem(SelectedMapItem);
                    sucess = true;
                }
            }
            catch (Exception ex)
            {
                Tools.Tool.WriteExceptionMessagesToOutputBox(ex);
                sucess = false;
            }

            if (CallFinished != null)
                CallFinished(sucess);
        }

        public delegate void Finish(bool sucess);
        public event Finish CallFinished;





    }

    public enum SpotDataState
    {
        Unknown,

        /// <summary>
        /// When no information were filled out yet.
        /// </summary>
        Clean,

        /// <summary>
        /// When the basic information were already filled out.
        /// </summary>
        BasicInfoInserted,

        /// <summary>
        /// When the full information were already filled out.
        /// </summary>
        FullInfoInserted
    }
}


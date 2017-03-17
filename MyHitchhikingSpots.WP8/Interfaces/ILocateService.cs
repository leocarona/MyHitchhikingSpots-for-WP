using System;
using System.Threading.Tasks;
using System.Threading;
using MyHitchhikingSpots.Interfaces;
//using MyHitchhikingSpots.Models;
using Xamarin.Geolocation;

namespace MyHitchhikingSpots.Interfaces
{
	public interface ILocateService
	{
		Task<Position> GetCurrentLocation();
		//Task<IMapItem> GetItemsLocation (IMapItem mapItem, CancellationToken cancellationToken = default(CancellationToken));
	}
}


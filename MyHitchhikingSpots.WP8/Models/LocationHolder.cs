using System;
using SQLite;
//using Xamarin.Geolocation;

namespace MyHitchhikingSpots.Models
{
	public class LocationHolder 
	{
		public string RequestString { get; set; }
		public string Country { get; set; }
		public string City { get; set; }
		[PrimaryKey]
		public string ID { get; set; }
		public LocationHolder() 
		{
			ID = Guid.NewGuid().ToString();
		}

		public double Accuracy
		{
			get;
			set;
		}

		public double Altitude
		{
			get;
			set;
		}

		public double AltitudeAccuracy
		{
			get;
			set;
		}

		public double Heading
		{
			get;
			set;
		}

		public double Latitude
		{
			get;
			set;
		}

		public double Longitude
		{
			get;
			set;
		}

		public double Speed
		{
			get;
			set;
		}

        public DateTime DateTime
        {
            get;
            set;
        }
	}
}


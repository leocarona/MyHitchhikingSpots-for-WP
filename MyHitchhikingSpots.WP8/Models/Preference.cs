using System;
using SQLite;
using MyHitchhikingSpots.ViewModels;

namespace MyHitchhikingSpots.Models
{
	public class Preference
	{
		string _prefkey;

		public string PrefKey { get { return PrefName.ToString (); } set { _prefkey = value; }}

		public SettingsKey PrefName { get; set; }

		public string PrefValue { get; set; }

		public PrefType PrefType { get; set; }
	}

	
}


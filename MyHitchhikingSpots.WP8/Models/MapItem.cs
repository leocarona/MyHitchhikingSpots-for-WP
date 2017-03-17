using System;
using SQLite;
using MyHitchhikingSpots.Interfaces;

namespace MyHitchhikingSpots.Models
{
    public class MapItem : IMapItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int MapId { get; set; }

        /// <summary>
        /// Name of the item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Item's street
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// Item's zip
        /// </summary>
        public string Zip { get; set; }

        /// <summary>
        /// Item's city
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Item's state
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Item's Country
        /// </summary>
        public string Country { get; set; }

        public double Longitude { get; set; }

        /// <summary>
        /// Latitude
        /// </summary>
        public double Latitude { get; set; }

        public double Distance { get; set; }

        /// <summary>
        /// If the coordinates was resolved 
        /// </summary>
        public bool GpsResolved
        {
            get
            {
                return (Latitude != 0 && Longitude != 0);
            }
        }

        public bool IsReverseGeocoded
        {
            get
            {
                return !String.IsNullOrEmpty(Street + Zip + City + State + Country);
            }
        }


        /// <summary>
        /// A personal note about anything (e.g. who picked you up, weather condition, how is it feeling to hitchike in this spot, etc).
        /// Note is not meant to be entirely the Description to be sent to the Hitchwiki maps itself. It can be used to add
        /// personal notes but of course some description may also go here to help you remembering later (like how to get to this spot, etc).
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// The Description to be shared on the Hitchwiki Maps
        /// </summary>
        public string Description { get; set; }

        public DateTime DateTime { get; set; }

        public long WaitingTimeTicks { get; set; }

        //This was necessary because SQLite does not support TimeSpans.
        public TimeSpan WaitingTime
        {
            get { return TimeSpan.FromTicks(WaitingTimeTicks); }
            set { WaitingTimeTicks = value.Ticks; }
        }


        public Hitchability Hitchability { get; set; }

        public AttemptResult AttemptResult { get; set; }
    }
}


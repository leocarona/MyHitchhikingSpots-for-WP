using SQLite;
using System;

namespace MyHitchhikingSpots.Interfaces
{
    public interface IMapItem
    {
        /// <summary>
        /// Unique id of the item
        /// </summary>
        [PrimaryKey, AutoIncrement]
        int Id { get; set; }

        int MapId { get; set; }

        /// <summary>
        /// Name of the item
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Item's street
        /// </summary>
        string Street { get; set; }

        /// <summary>
        /// Item's zip
        /// </summary>
        string Zip { get; set; }

        /// <summary>
        /// Item's city
        /// </summary>
        string City { get; set; }

        /// <summary>
        /// Item's state
        /// </summary>
        string State { get; set; }

        /// <summary>
        /// Item's Country
        /// </summary>
        string Country { get; set; }

        /// <summary>
        /// Longtitude
        /// </summary>
        double Longitude { get; set; }

        /// <summary>
        /// Latitude
        /// </summary>
        double Latitude { get; set; }

        /// <summary>
        /// Distance
        /// </summary>
        double Distance { get; set; }

        /// <summary>
        /// If the coordinates are both set 
        /// </summary>
        bool GpsResolved { get; }


        /// <summary>
        /// A personal note about anything (e.g. who picked you up, weather condition, how is it feeling to hitchike in this spot, etc).
        /// Note is not meant to be entirely the Description to be sent to the Hitchwiki maps itself. It can be used to add
        /// personal notes but of course some description may also go here to help you remembering later (like how to get to this spot, etc).
        /// </summary>
        string Note { get; set; }

        /// <summary>
        /// The Description to be shared on the Hitchwiki Maps
        /// </summary>
        string Description { get; set; }

        DateTime DateTime { get; set; }

        long WaitingTimeTicks { get; set; }

        TimeSpan  WaitingTime { get; set; }

        Hitchability Hitchability { get; set; }

        AttemptResult AttemptResult { get; set; }
    }



    public enum Hitchability
    {
        Unknown,
        VeryGood,
        Good,
        Average,
        Bad,
        Senseless,
        NoAnswer
    }

    public enum AttemptResult
    {
        Unknown,
        Successful,
        Gaveup,
    }
}


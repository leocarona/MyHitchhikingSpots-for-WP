using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyHitchhikingSpots.Interfaces;
using MyHitchhikingSpots.Resources;

namespace MyHitchhikingSpots.ViewModels
{
    public partial class MapViewModel : ViewModelBase
    {
        public List<Tuple<Hitchability, String>> HitchabilityOptions
        {
            get
            {
                var options = new List<Tuple<Hitchability, String>>();

                options.Add(new Tuple<Hitchability, String>(Hitchability.NoAnswer, AppResources.HitchabilityNoAnswerOption));
                options.Add(new Tuple<Hitchability, String>(Hitchability.VeryGood, AppResources.HitchabilityVeryGoodOption));
                options.Add(new Tuple<Hitchability, String>(Hitchability.Good, AppResources.HitchabilityGoodOption));
                options.Add(new Tuple<Hitchability, String>(Hitchability.Average, AppResources.HitchabilityAverageOption));
                options.Add(new Tuple<Hitchability, String>(Hitchability.Bad, AppResources.HitchabilityBadOption));
                options.Add(new Tuple<Hitchability, String>(Hitchability.Senseless, AppResources.HitchabilitySenselessOption));

                return options;
            }
        }

        public List<Tuple<AttemptResult, String>> AttemptResultOptions
        {
            get
            {
                var options = new List<Tuple<AttemptResult, String>>();

                options.Add(new Tuple<AttemptResult, String>(AttemptResult.Successful, AppResources.AttemptResultGotARideOption));
                options.Add(new Tuple<AttemptResult, String>(AttemptResult.Gaveup, AppResources.AttemptResultGaveUpOption));

                return options;
            }
        }

        public List<Tuple<int, String>> WaitingTimeDefaultOptions
        {
            get
            {
                var options = new List<Tuple<int, String>>();

                options.Add(new Tuple<int, String>(-1, "N/A"));
                options.Add(new Tuple<int, String>(5, "5 min"));
                options.Add(new Tuple<int, String>(10, "10 min"));
                options.Add(new Tuple<int, String>(15, "15 min"));
                options.Add(new Tuple<int, String>(20, "20 min"));
                options.Add(new Tuple<int, String>(30, "30 min"));
                options.Add(new Tuple<int, String>(45, "45 min"));
                options.Add(new Tuple<int, String>(60, "1 hour"));

                return options;
            }
        }
    }
}

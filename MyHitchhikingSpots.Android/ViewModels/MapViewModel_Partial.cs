using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyHitchhikingSpots.Interfaces;

namespace MyHitchhikingSpots.ViewModels
{
    public partial class MapViewModel : ViewModelBase
    {
        public void SetOptions(List<Tuple<Hitchability, String>> hitchability, List<Tuple<AttemptResult, String>> attemptResult)
        {
            HitchabilityOptions = hitchability;
            AttemptResultOptions = attemptResult;
        }

        public List<Tuple<Hitchability, String>> HitchabilityOptions { get; private set; }

        public List<Tuple<AttemptResult, String>> AttemptResultOptions { get; private set; }



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

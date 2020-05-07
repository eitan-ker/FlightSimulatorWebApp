using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb
{
    public class Segment
    {
        private double _Longtitude;
        private double _Latitude;
        private int _Timespan_seconds;
        public Segment(double Longtitude, double Latitude, int Timespan_seconds)
        {
            this._Latitude = Latitude;
            this._Longtitude = Longtitude;
            this._Timespan_seconds = Timespan_seconds;
        }
    }
}

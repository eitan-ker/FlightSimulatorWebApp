using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb
{
    public class FlightPlan
    {
        private int _Passengers;
        private string _Company_Name;
        Segment _Initial_Location;
        private List<Segment> SegmentsList;
        public FlightPlan(int Passengers, string Company_Name, Segment Initial_Location, List<Segment> list) {
            this._Passengers = Passengers;
            this._Company_Name = Company_Name;
            this._Initial_Location = Initial_Location;
            this.SegmentsList = list;
        }
    }
}

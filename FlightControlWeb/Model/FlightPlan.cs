using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb
{
    public class FlightPlan
    {

        public int Passengers { get; set; }
        public string Company_Name { get; set; }
        public Segment Initial_Location { get; set; }
        public List<Segment> SegmentsList { get; set; }
      
    }
}

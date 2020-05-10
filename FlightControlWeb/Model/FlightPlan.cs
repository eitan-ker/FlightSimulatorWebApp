using System.Collections.Generic;

namespace FlightControlWeb.Model
{
    public class FlightPlan
    {
        public string ID { get; set; }
        public int Passengers { get; set; }
        public string Company_Name { get; set; }
        public InitialLocation Initial_Location{ get; set; }
        public IEnumerable<Segment> Segments { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb
{
    public class Flight
    {
        public DateTime Date_time { get; set; }

        public string FlightID { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public string Company_name { get; set; }
        public bool Is_external { get; set; }
    }
}

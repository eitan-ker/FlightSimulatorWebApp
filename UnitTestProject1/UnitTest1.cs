using FlightControlWeb;
using FlightControlWeb.Controllers;
using FlightControlWeb.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.IO;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private readonly IMemoryCache _cache;

        [TestMethod]
        public void TestMethod1() // test Server class
        {
            // Arrange
           
            Flight test_flight1 = new Flight("testId", 32, 32, 100, "Test_Airways", DateTime.UtcNow, false);
            Flight test_flight2 = new Flight();

            // Act
            test_flight2.SetFlight("flight_id", test_flight1.FlightID);
            test_flight2.SetFlight("longitude", Convert.ToString(test_flight1.Longitude));
            test_flight2.SetFlight("latitude", Convert.ToString(test_flight1.Latitude));
            test_flight2.SetFlight("passengers", Convert.ToString(test_flight1.Passengers));
            test_flight2.SetFlight("company_name", test_flight1.Company_name);
            test_flight2.SetFlight("date_time", Convert.ToString(test_flight1.Date_time));
            test_flight2.SetFlight("is_external", Convert.ToString(test_flight1.Is_external));

            // Assert
            if (!isEqualFlights(test_flight1, test_flight2))
            {
                Assert.Fail();
            }
        }
        bool isEqualFlights(Flight f1, Flight f2)
        {
            if (f1.FlightID.CompareTo(f2.FlightID) != 0) { return false; }
            if (f1.Longitude != f2.Longitude) { return false; }
            if (f1.Latitude != f2.Latitude) { return false; }
            if (f1.Passengers != f2.Passengers) { return false; }
            if (f1.Company_name.CompareTo(f2.Company_name) != 0) { return false; }
            string f1_date = Convert.ToString(f1.Date_time);
            string f2_date = Convert.ToString(f2.Date_time);
            if (f1_date.CompareTo(f2_date) != 0) { return false; }
            if (f1.Is_external != f2.Is_external) { return false; }
            return true;
        }
    }
}

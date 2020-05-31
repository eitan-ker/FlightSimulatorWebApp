using FlightControlWeb;
using FlightControlWeb.Controllers;
using FlightControlWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using System.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1

    {      

        [TestMethod]
        public void SetFlightTest_TwoSameFlight_ReturnsTrue_() // test Server class
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
            if (!IsEqualFlights(test_flight1, test_flight2))
            {
                Assert.Fail();
            }
        }
        private bool IsEqualFlights(Flight f1, Flight f2)
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

        [TestMethod]
        public void GetServersList_SameServers_ReturnsTrue()
        {
            // Arrange
            var cache_Test = new MemoryCache(new MemoryCacheOptions());
            string serJson = File.ReadAllText("./server.json");
            Server server = JsonConvert.DeserializeObject<Server>(serJson);
            var Server_Mock = new Mock<IServerController>();

            // Act
            Server_Mock.Setup(x => x.GetServersList()).Returns(new List<Server>(new List<Server>(1) { server }));
            List<Server> sl = Server_Mock.Object.GetServersList();

            // Assert
            Assert.IsTrue(CheckServerTest(server, sl));
        }

        private bool CheckServerTest(Server server, List<Server> ServerList)
        {
            foreach(Server ser in ServerList)
            {
                if (server.ServerId.CompareTo(ser.ServerId) != 0) { return false; }
                if (server.ServerUrl.CompareTo(ser.ServerUrl) != 0) { return false; }
            }
            return true;
        }
    }
}

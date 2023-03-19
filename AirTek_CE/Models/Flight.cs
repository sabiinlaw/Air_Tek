using System.Runtime.CompilerServices;

namespace AirTek_CE
{
    public class Flight
    {
        public string From { get; set; }
        public string To { get; set; }
        public string FromCode { get; set; }
        public string ToCode { get; set; }
        public int Capacity { get; set; }
    }

    public class FlightInfo
    {
        public int Number { get; set; }
        public string Departure { get; set; }
        public string Arival { get; set; }
        public int Day { get; set; }
        public int Capacity { get; set; }

    }
}
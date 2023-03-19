using AirTek_CE.FlightProcessing;
using System;

namespace AirTek_CE
{
    public class Program
    {
        static void Main(string[] args)
        {
            var flightService = new FlightSchedulleService();

            //user_story_1
            var flightsScenario = flightService.GetFlightSchedule();
            var flightSchedulesList = flightService.GetFlightSchedules(flightsScenario);

            foreach (var schedule in flightSchedulesList)
            {
                Console.WriteLine($"flight: {schedule.Number}, " +
                                  $"departure: {schedule.Departure}, " +
                                  $"arrival: {schedule.Arival}, " +
                                  $"day: {schedule.Day}");
            }

            //user_story_2
            var orders = flightService.GetOrders(@"..\..\DataSource\coding-assigment-orders.json", 50);
            var ordersByFlight = flightService.GetFlightByOrders(orders);            

            foreach (var order in ordersByFlight)
            {
                var message = order.Flight != null
                    ? $"order: {order.Name}, flightNumber: {order.Flight.Number}, departure: {order.Flight.Departure}, arrival: {order.Flight.Arival}, day: {order.Flight.Day}"
                    : $"order: {order.Name}, flightNumber: not scheduled";

                Console.WriteLine(message);
            }

            Console.ReadKey();
        }
    }
}

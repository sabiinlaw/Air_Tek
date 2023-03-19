using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using System.Linq;

namespace AirTek_CE.FlightProcessing
{
    public class FlightSchedulleService
    {
        public Dictionary<int, List<Flight>> GetFlightSchedule()
        {
            var flightsScenario = new Dictionary<int, List<Flight>>
            {
                {
                    1,
                    new List<Flight>()
                    {
                         new Flight() { From = "Montreal", FromCode = "YUL", To = "Toronto", ToCode = "YYZ", Capacity = 20 },
                         new Flight() { From = "Montreal", FromCode = "YUL", To = "Calgary", ToCode = "YYC", Capacity = 20 },
                         new Flight() { From = "Montreal", FromCode = "YUL", To = "Vancouver", ToCode = "YVR",Capacity = 20 }
                    }
                },
                {
                    2,
                    new List<Flight>()
                    {
                        new Flight() { From = "Montreal", FromCode = "YUL", To = "Toronto", ToCode = "YYZ",Capacity = 20 },
                        new Flight() { From = "Montreal", FromCode = "YUL", To = "Calgary", ToCode = "YYC", Capacity = 20 },
                        new Flight() { From = "Montreal", FromCode = "YUL", To = "Vancouver", ToCode = "YVR", Capacity = 20 }
                    }
                }
            };
            return flightsScenario;
        }

        #region user_story_1
        public IEnumerable<FlightInfo> GetFlightSchedules(Dictionary<int, List<Flight>> flightsScenario, Expression<Func<FlightInfo, bool>> filter = null)
        {
            var flightList = new List<FlightInfo>();
            try
            {
                var flightCounter = 0;
                foreach (var item in flightsScenario)
                {
                    foreach (var subItem in item.Value)
                    {
                        flightCounter++;
                        flightList.Add(new FlightInfo()
                        {
                            Number = flightCounter,
                            Departure = subItem.FromCode,
                            Arival = subItem.ToCode,
                            Capacity = subItem.Capacity,
                            Day = item.Key
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Can't get flight schedule info", ex);
            }

            return filter != null
                ? (IEnumerable<FlightInfo>)flightList.AsQueryable().Where(filter)
                : flightList;
        }

        public Dictionary<string, FlightInfo[]> GetFlightSchedulesByDestination(string[] destinations)
        {
            var flightListByDestination = new Dictionary<string, FlightInfo[]>();

            try
            {
                var flightScenario = GetFlightSchedule();
                foreach (var destination in destinations)
                {
                    var flightInfoes = GetFlightSchedules(flightScenario, x => destination.Equals(x.Arival)).ToArray();
                    if (!flightListByDestination.ContainsKey(destination))
                        flightListByDestination.Add(destination, flightInfoes);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Can't get flight schedule infos by provided destinations: [{string.Join(",", destinations)}", ex);
            }

            return flightListByDestination;
        }
        #endregion

        #region user_story_2
        public IEnumerable<Order> GetOrders(string filePath, int batchSize)
        {
            try
            {
                var orders = new List<Order>();
                var reader = new CustomFileReader(filePath, batchSize);

                foreach (var batch in reader)
                {
                    foreach (var item in batch)
                    {
                        orders.Add(item);
                    }
                }

                return orders;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Can't get order list from: {filePath}", ex);
            }
        }

        public List<(string Name, FlightInfo Flight)> GetFlightByOrders(IEnumerable<Order> orders)
        {
            var destinationCapacities = new Dictionary<string, int>();
            var flightsByOrders = new List<(string Name, FlightInfo Flight)>();

            var flightInfoesByDestination = GetFlightSchedulesByDestination(orders.Select(x => x.Destination).ToArray());

            try
            {
                foreach (var order in orders)
                {
                    if (flightInfoesByDestination.TryGetValue(order.Destination, out FlightInfo[] flights) && flights.Length > 0)
                    {
                        if (destinationCapacities.TryGetValue(order.Destination, out int desctinationCurrentCapacity))
                        {
                            var flightsCapacitySum = 0;
                            var capacity = desctinationCurrentCapacity;
                            var i = 0;

                            do
                            {
                                flightsCapacitySum += flights[i].Capacity;
                                if (capacity < flightsCapacitySum)
                                {
                                    flightsByOrders.Add((order.Name, flights[i]));
                                    break;
                                }

                                i++;

                                if (i == flights.Length)
                                {
                                    capacity -= flightsCapacitySum;
                                }

                            } while (capacity > 0);


                            destinationCapacities[order.Destination] = ++desctinationCurrentCapacity;
                        }
                        else
                        {
                            flightsByOrders.Add((order.Name, flights[0]));
                            destinationCapacities[order.Destination] = ++desctinationCurrentCapacity;
                        }
                    }
                    else
                    {
                        flightsByOrders.Add((order.Name, null));
                    }
                }
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException($"Can't get flights by orders", ex);
            }

            return flightsByOrders;
        }

        #endregion
    }
}

namespace FlightRoutePlanner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;

    public class Flight 
    {
        public string FromCity { get; set; }
        public string ToCity { get; set; }
        public int Distance { get; set; }
        public int SeatsLeft { get; set; }
        public bool IsPeakHour { get; set; }

        public int Discount { get; set; }

        public decimal GetFlightPrice() 
        {
            decimal basePrice = Distance * 0.1m;
            decimal dynamicPrice = basePrice;
            if (IsPeakHour) 
            {
                dynamicPrice *= 1.4m;
            }
            if (SeatsLeft < 10) 
            {
                dynamicPrice *= 1.2m;
            }
            if (Discount > 0) // Assuming sometimes there will be no discount
            {
                dynamicPrice -= dynamicPrice * (Discount / 100.00m);
            }

            return dynamicPrice;
        }
    }

    public class FlightRoutePlanner 
    {
        private Dictionary<string, List<Flight>> flights = new Dictionary<string, List<Flight>>();

        public void AddFlight(Flight flight) 
        {
            if (!flights.ContainsKey(flight.FromCity)) 
            {
                flights[flight.FromCity] = new List<Flight>();
            }

            flights[flight.FromCity].Add(flight);
        }

        public List<string> GetCheapestRoute(string start, string end) 
        {
            var cheapestPrice = new Dictionary<string, decimal>();
            var previousCity = new Dictionary<string, string>();
            var priorityQueue = new SortedSet<(decimal, string)>();

            foreach (var city in flights.Keys) 
            {
                cheapestPrice[city] = decimal.MaxValue;
                previousCity[city] = null;

                foreach (var flight in flights[city])
                {
                    if (!cheapestPrice.ContainsKey(flight.ToCity)) 
                    {
                        cheapestPrice[flight.ToCity] = decimal.MaxValue;
                        previousCity[flight.ToCity] = null;
                    }
                }
            }
            cheapestPrice[start] = 0;
            priorityQueue.Add((0.0m, start));

            while (priorityQueue.Count > 0) 
            {
                var (currentPrice, currentCity) = priorityQueue.Min;
                priorityQueue.Remove(priorityQueue.Min);

                if (currentCity == end) 
                {
                    break;
                }

                if (flights.ContainsKey(currentCity))
                {
                    foreach (var flight in flights[currentCity])
                    {
                        decimal price = currentPrice + flight.GetFlightPrice();
                        if (price < cheapestPrice[flight.ToCity])
                        {
                            priorityQueue.Remove((cheapestPrice[flight.ToCity], flight.ToCity));
                            cheapestPrice[flight.ToCity] = price;
                            previousCity[flight.ToCity] = currentCity;
                            priorityQueue.Add((price, flight.ToCity));
                        }
                    }
                }
                
            }

            var route = new List<string>();
            for (var at = end; at != null; at = previousCity.GetValueOrDefault(at)) 
            {
                route.Add(at);
            }
            route.Reverse();
            return route;
        }

        public List<string> GetRoundTripWithOneConnection(string start, string end)
        {
            var forwardRoute = GetCheapestRoute(start, end);
            var returnRoute = GetCheapestRoute(end, start);

            if (forwardRoute.Count > 3 || returnRoute.Count > 3)
            {
                return null;
            }

            forwardRoute.AddRange(returnRoute.Skip(1)); // Combine forward and return routes
            return forwardRoute;
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            var planner = new FlightRoutePlanner();

            planner.AddFlight(new Flight { FromCity = "A", ToCity = "B", Distance = 500, SeatsLeft = 20, IsPeakHour = false, Discount = 5 });
            planner.AddFlight(new Flight { FromCity = "B", ToCity = "C", Distance = 300, SeatsLeft = 5, IsPeakHour = true, Discount = 3 });
            planner.AddFlight(new Flight { FromCity = "A", ToCity = "C", Distance = 800, SeatsLeft = 15, IsPeakHour = false, Discount = 8 });

            var routeAC = planner.GetCheapestRoute("A", "C");
            Console.WriteLine("Cheapest route from A to C is " + string.Join(" -> ", routeAC));

            var routeAB = planner.GetRoundTripWithOneConnection("A", "B");
            if (routeAB != null)
            {
                Console.WriteLine("Round-trip route from A to B with at most one connection: " + string.Join(" -> ", routeAB));
            }
            else
            {
                Console.WriteLine("No valid round-trip route  from A to B with at most one connection found.");
            }
        }
    }
}

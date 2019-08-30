using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NextBusFncApp.Classes;
using NateK.Google.Assistant;
using NextBusFncApp.Models;
using NateK.BCTransit;
using NateK.BCTransit.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace NextBusFncApp
{
    public class NextBus
    {
        private const string WorkStopIdKey = "Locations:Work:StopId";
        private const string WorkRouteCodeKey = "Locations:Work:RouteCode";
        private const string WorkLatKey = "Locations:Work:Lat";
        private const string WorkLngKey = "Locations:Work:Lng";

        private const string HomeStopIdKey = "Locations:Home:StopId";
        private const string HomeRouteCodeKey = "Locations:Home:RouteCode";
        private const string HomeLatKey = "Locations:Home:Lat";
        private const string HomeLngKey = "Locations:Home:Lng";

        private readonly IBCTransitRouteSchedule _bcTransitRouteSchedule;
        private readonly IConfigurationRoot _configuration;
        private ILogger _log;

        public NextBus(IBCTransitRouteSchedule bcTransitRouteSchedule, IConfigurationRoot config)
        {
            _bcTransitRouteSchedule = bcTransitRouteSchedule;
            _configuration = config;
        }


        [FunctionName("NextBus")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            _log = log;
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<DialogFulfillmentRequest<GenericActionParameters>>(requestBody);

            if (data == null ||
                data.QueryResult == null ||
                data.QueryResult.Parameters == null)
            {
                return new OkObjectResult(new DialogFulfillmentResponse()
                {
                    FulfillmentText = "Could not help you - no parameters provided"
                });
            }
            try
            {
                var action = data.QueryResult.Parameters.Action;
                if (string.IsNullOrWhiteSpace(action))
                {
                    // default type = return what the next bus is
                    var nextBusData = JsonConvert.DeserializeObject<DialogFulfillmentRequest<NextBusParameters>>(requestBody);
                    return await HandleNextBusAction(nextBusData);
                }

                switch (action.ToLower())
                {
                    case "define-load":
                        var nextBusData = JsonConvert.DeserializeObject<DialogFulfillmentRequest<NextBusLoadParameters>>(requestBody);
                        return await HandleNextBusLoadAction(nextBusData);

                }

                return new BadRequestObjectResult(new DialogFulfillmentResponse()
                {
                    FulfillmentText = "Could not determine action"
                });
            }
            catch (OverflowException e)
            {
                _log.LogError(e, e.Message);
                return new BadRequestObjectResult(new DialogFulfillmentResponse()
                {
                    FulfillmentText = "Location could not be parsed"
                });
            }
            catch (Exception e)
            {
                _log.LogError(e, e.Message);
                return new BadRequestObjectResult(new DialogFulfillmentResponse()
                {
                    FulfillmentText = e.Message
                });
            }
        }

        private async Task<IActionResult> HandleNextBusLoadAction(DialogFulfillmentRequest<NextBusLoadParameters> data)
        {
            var originalParameters = data.QueryResult.OutputContexts.First(x => x.Name.EndsWith("route-data"));
            if (originalParameters == null) throw new Exception("Could not determine route pattern");
            var parameters = new NextBusLoadParameters()
            {
                Location = originalParameters.Parameters.location,
                PatternId = originalParameters.Parameters.patternId
            };

            string stopId;
            string routeCode;
            Locations location;
            ValidateLocationParameter(parameters);
            GetStopIdAndRouteCode(parameters, out stopId, out routeCode, out location);

            if (parameters.PatternId == 0) throw new Exception("Could not determine route pattern");
            var vehicleStatuses = await _bcTransitRouteSchedule.GetVehicleStatuses(parameters.PatternId);

            var busInfo = GetNextBusFromLocation(location, vehicleStatuses);
            var message = "Bus # " + busInfo.Name + " " +  busInfo.HeadsignText;
            if (!string.IsNullOrWhiteSpace(busInfo.HeadsignText))
            {
                switch (busInfo.vehicleCapacityIndicator.ToUpper())
                {
                    case "GREEN":
                        message += " has low load";
                        break;
                    case "YELLOW":
                        message += " has medium load";
                        break;
                    case "RED":
                        message += " has heavy load";
                        break;
                }
            }
            else
            {
                message += " has no load data";
            }

            message += $" and is going {busInfo.Velocity} km/h";

            return new OkObjectResult(new DialogFulfillmentResponse()
            {
                FulfillmentText = message
            });
        }

        private VehicleStatusesData GetNextBusFromLocation(Locations location, List<VehicleStatusesData> vehicleStatuses)
        {
            Direction dir;
            decimal lat, lng;
            if (location == Locations.HOME)
            {
                lat = Decimal.Parse(_configuration[HomeLatKey]);
                lng = Decimal.Parse(_configuration[HomeLngKey]);
                dir = Direction.West; // direction bus is coming from
            }
            else
            {
                lat = Decimal.Parse(_configuration[WorkLatKey]);
                lng = Decimal.Parse(_configuration[WorkLngKey]);
                dir = Direction.East; // direction bus is coming from
            }

            if (dir == Direction.West) return vehicleStatuses.OrderBy(x => x.Lng).FirstOrDefault(x => x.Lng < lng);
            else return vehicleStatuses.OrderBy(x => x.Lng).FirstOrDefault(x => x.Lng > lng); // east
        }

        private async Task<IActionResult> HandleNextBusAction(DialogFulfillmentRequest<NextBusParameters> data)
        {
            string stopId;
            string routeCode;
            Locations location;
            ValidateLocationParameter(data.QueryResult.Parameters);
            GetStopIdAndRouteCode(data.QueryResult.Parameters, out stopId, out routeCode, out location);

            int patternId = 0;
            List<StopPredictionData> results;
            try
            {
                var stopData = await _bcTransitRouteSchedule.GetStopPredictionsForRoute(stopId, routeCode);
                results = stopData.Item1;
                patternId = stopData.Item2;

                if (results.Count == 0)
                {
                    return new OkObjectResult(new DialogFulfillmentResponse()
                    {
                        FulfillmentText = $"No scheduled busses found"
                    });
                }

                var firstBus = results[0];

                string message = "";
                if (firstBus.PredictionType == PredictionTypes.Predicted.ToString())
                {
                    var now = DateTime.Now;
                    var diff = results[0].PredictTime - now;
                    var minutes = diff.Minutes;
                    message = $"Next bus comes in {minutes} minutes.";
                    if (firstBus.PredictTime > firstBus.ScheduleTime)
                    {
                        var timeDiff = firstBus.PredictTime - firstBus.ScheduleTime;
                        if (timeDiff.Minutes > 2)
                        {
                            message += $" The bus is running later than scheduled by {timeDiff.Minutes} minutes.";
                        }
                    }
                    else if (firstBus.PredictTime < firstBus.ScheduleTime)
                    {
                        var timeDiff = firstBus.ScheduleTime - firstBus.PredictTime;
                        if (timeDiff.Minutes > 2)
                        {
                            message += $" The bus is running earlier than scheduled by {timeDiff.Minutes} minutes.";
                        }
                    }
                }
                else
                {
                    message = $"Next bus is scheduled for {GetTimeStamp(firstBus.PredictTime)}.";
                }

                if (results.Count > 1)
                {
                    var secondBusDiff = results[1].PredictTime - DateTime.Now;
                    message += $" The following bus comes in {secondBusDiff.Minutes} minutes at {GetTimeStamp(results[1].PredictTime)}";
                }

                return new OkObjectResult(new DialogFulfillmentResponse()
                {

                    FulfillmentText = message,
                    OutputContexts = new List<DialogFulfullmentOutputContext>()
                    {
                        new DialogFulfullmentOutputContext
                        {
                            LifespanCount = 1,
                            Name = data.Session + "/contexts/route-data",
                            Parameters = new
                            {
                                PatternId = patternId,
                                Location = location.ToString()
                            }
                        }
                    }
                });
            }
            catch (Exception e)
            {
                _log.LogError(e.Message, e);
                return new OkObjectResult(new DialogFulfillmentResponse()
                {
                    FulfillmentText = $"Error fetching next bus"
                });
            }
        }

        private void GetStopIdAndRouteCode(NextBusParameters parameters, out string stopId, out string routeCode, out Locations location)
        {
            var locationStr = parameters.Location.ToUpper();
            location = Enum.Parse<Locations>(locationStr);
            switch (location)
            {
                case Locations.WORK:
                    stopId = _configuration[WorkStopIdKey];
                    routeCode = _configuration[WorkRouteCodeKey];
                    break;
                case Locations.HOME:
                    stopId = _configuration[HomeStopIdKey];
                    routeCode = _configuration[HomeRouteCodeKey];
                    break;
                default:
                    throw new Exception("Location not supported");
            }
        }

        private static void ValidateLocationParameter(NextBusParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.Location))
            {
                throw new ArgumentException("Need a to or from location");
            }
        }

        private static string GetTimeStamp(DateTime date)
        {
            return date.ToString("hh:mm tt");
        }
    }
}

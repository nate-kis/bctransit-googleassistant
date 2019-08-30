using NateK.BCTransit.Models;
using NateK.Lib;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Linq;

namespace NateK.BCTransit
{
    public interface IBCTransitRouteSchedule
    {
        Task<Tuple<List<StopPredictionData>, int>> GetStopPredictionsForRoute(string stopId, string routeCode);
        Task<List<VehicleStatusesData>> GetVehicleStatuses(int patternId);
    }

    public class BCTransitRouteSchedule : IBCTransitRouteSchedule
    {
        private const string PredictionDataApi = "https://nextride.victoria.bctransit.com/api/PredictionData";
        private const string VehicleStatusesApi = "https://nextride.victoria.bctransit.com/api/VehicleStatuses";
        private readonly IHttpService _httpService;

        public BCTransitRouteSchedule(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<Tuple<List<StopPredictionData>, int>> GetStopPredictionsForRoute(string stopId, string routeCode)
        {
            var result = await _httpService.Get<PredictionDataResult>(PredictionDataApi + "?shouldLog=false&stopId=" + HttpUtility.UrlEncode(stopId));
            var routeData = result.GrpByPtrn.Where(x => x.RouteCode == routeCode).FirstOrDefault();
            int patternId = routeData?.PatternId ?? 0;
            return new Tuple<List<StopPredictionData>, int>(routeData?.Predictions, patternId);
        }

        public async Task<List<VehicleStatusesData>> GetVehicleStatuses(int patternId)
        {
            var result = await _httpService.Get<List<VehicleStatusesData>>(VehicleStatusesApi + "?shouldLog=false&patternIds[]=" + HttpUtility.UrlEncode(patternId.ToString()));
            return result;
        }


    }
}

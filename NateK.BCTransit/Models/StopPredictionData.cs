using System;
using System.Collections.Generic;
using System.Text;

namespace NateK.BCTransit.Models
{
    public class StopPredictionData
    {
        public DateTime PredictTime { get; set; }
        public DateTime ScheduleTime { get; set; }
        public string PredictionType { get; set; }
        public int SeqNo { get; set; }
    }

    public class PredictionDataResult
    {
        public string StopCode { get; set; }
        public List<PredictionDataGroup> GrpByPtrn { get; set; }
    }

    public class PredictionDataGroup
    {
        public string RouteName { get; set; }
        public string RouteCode { get; set; }
        public int PatternId { get; set; }
        public List<StopPredictionData> Predictions { get; set; }
    }
}

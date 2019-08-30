using System;
using System.Collections.Generic;
using System.Text;

namespace NateK.BCTransit.Models
{
    public class VehicleStatusesData
    {
        public int PatternId { get; set; }
        public string vehicleCapacityIndicator { get; set; }
        public bool IsLight { get; set; }
        public bool IsMedium { get; set; }
        public bool IsFull { get; set; }
        public string HeadsignText { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public decimal Velocity { get; set; }
        public string Name { get; set; }
    }
}

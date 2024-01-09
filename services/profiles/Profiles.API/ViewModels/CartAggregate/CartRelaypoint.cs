﻿using Profiles.API.ViewModels.Relaypoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels.CartAggregate
{
    public class CartRelaypoint
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public string Location { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }

        public string WorkingDay { get; set; }
        public string WorkingTime { get; set; }
        public float Rating { get; set; }
        public string CoverImageUrl { get; set; }
        public string ProfileImageUrl { get; set; }

        public double DistanceFromOrigin { get; set; }

        public List<WorkingDaysModel> WorkingDaysList { get; set; }
    }
}

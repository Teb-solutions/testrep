using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EasyGas.Services.Profiles.Models
{
    public class Settings : Trackable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TenantId { get; set; }
        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }
        public int? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; }
        
        public bool GeoFenceOrderCreation { get; set; } // whether to check if order address is geofenced while creating orders
        public bool AutoAssignVehicle { get; set; } // if true planning algo will be run on order creation
        public bool SlotThresholdLimit { get; set; } // if true threshold limit rules will be applied to make slots active/inactive
        public bool CheckVehLocUpdateThreshold { get; set; } // if this is true, vehs will be selected based on VehLocUpdateThresholdSec for planning
        public int VehLocUpdateThresholdSec { get; set; } // all vehs whose location last updated time is within last VehLocUpdateThresholdSec secs will be used for planning, applied only if CheckVehLocUpdateThreshold is true 
        public bool TakeCurrentVehLocForPlanning { get; set; } // if true, realtime veh loc is taken for planning, else origin latlng from veh table is taken
        public bool CheckVehLoginStateForPlanning { get; set; } // if true, only veh with last login state as login/resume will be taken for planning
        public bool CheckVehLocIsGeoFencedForPlanning { get; set; } // if true, only vehs which are inside branch geofence will be taken for planning, geofence is calculated from VehLocMaxDistanceFromBranchMet radius from branch latlng
        public int VehLocMaxDistanceFromBranchMet { get; set; } // applied only if CheckVehLocIsGeoFencedForPlanning = true
        public bool AutoPlanManuallyAssigned { get; set; } // if true, manually assigned orders will be replanned
        //public bool ReAssignRejectedOrder { get; set; } // if false, rejected drivers wont receive the same order within rejecetion threshold time
        public int? ReAssignRejectedOrderThresholdSec { get; set; } // if > 0, rejected drivers wont receive the same order within rejecetion threshold time


        // broadcast
        public bool BroadcastCheckVehLoginState { get; set; }
        public bool BroadcastVehLocIsGeoFenced { get; set; }
        public int BroadcastRadiusMet { get; set; }
        public int BroadcastTimeOutSec { get; set; }
        public int BroadcastTimeOutBackendBufferSec { get; set; }
        public int BroadcastTimeOutCustomerBufferSec { get; set; }
        public bool BroadcastTakeCurrentVehLoc { get; set; }
        public bool BroadcastCheckVehLocUpdateThreshold { get; set; } 
        public int BroadcastVehLocUpdateThresholdSec { get; set; } 
        public bool BroadcastToDispatchedVeh { get; set; }
    }
}
